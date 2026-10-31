using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.SuivisJournaliers.Dtos;
using Application.Features.Bandes.Interfaces;
using Application.Common.Mappings;

namespace Application.Features.SuivisJournaliers.Queries.GetAllSuivisByBandeId
{
    /// <summary>
    /// Handler pour récupérer tous les suivis d'une bande. Applique les filtres optionnels et trie les résultats.
    /// </summary>
    public class GetAllSuivisByBandeIdQueryHandler 
        : IRequestHandler<GetAllSuivisByBandeIdQuery, List<SuiviResponseDto>>
    {
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly ILogger<GetAllSuivisByBandeIdQueryHandler> _logger;

        public GetAllSuivisByBandeIdQueryHandler(
            ISuiviJournalierRepository suiviRepository,
            IBandeRepository bandeRepository,
            ILogger<GetAllSuivisByBandeIdQueryHandler> logger)
        {
            _suiviRepository = suiviRepository;
            _bandeRepository = bandeRepository;
            _logger = logger;
        }

        public async Task<List<SuiviResponseDto>> Handle(
            GetAllSuivisByBandeIdQuery request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Récupération des suivis pour la bande {BandeId}", 
                request.BandeId);

            // ============================================
            // 1. Vérifier que la bande existe
            // ============================================
            var bande = await _bandeRepository.GetByIdAsync(request.BandeId, cancellationToken);
            if (bande == null)
            {
                throw new NotFoundException($"Bande avec l'ID {request.BandeId} non trouvée.");
            }

            // ============================================
            // 2. Récupérer les suivis (avec ou sans filtre de dates)
            // ============================================
            IEnumerable<Domain.Entities.SuiviJournalier> suivis;

            if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                suivis = await _suiviRepository.GetByBandeIdAndDateRangeAsync(
                    request.BandeId,
                    request.DateDebut.Value,
                    request.DateFin.Value,
                    cancellationToken);
            }
            else
            {
                suivis = await _suiviRepository.GetByBandeIdAsync(
                    request.BandeId, 
                    cancellationToken);
            }

            // ============================================
            // 3. Appliquer les filtres supplémentaires
            // ============================================
            if (request.OnlyWithSoins)
            {
                suivis = suivis.Where(s => s.TypeSoin.HasValue);
            }

            if (request.OnlyWithMortalites)
            {
                suivis = suivis.Where(s => s.NbMortalites > 0);
            }

            // ============================================
            // 4. Trier par jour
            // ============================================
            suivis = suivis.OrderBy(s => s.Jour);

            _logger.LogInformation(
                "{Count} suivis trouvés pour la bande {BandeId}", 
                suivis.Count(), 
                request.BandeId);

            // ============================================
            // 5. Mapper et retourner
            // ============================================
            var suivisDto = suivis.ToResponseDtos();

            // Ajouter des informations supplémentaires si nécessaire
            foreach (var dto in suivisDto)
            {
                dto.DateArriveeBande = bande.DateArrivee;
                // Note: D'autres propriétés comme BandeCode, StockConsommeNom, etc.
                // seront remplies par le repository via les JOINs SQL
            }

            return suivisDto;
        }
    }
}