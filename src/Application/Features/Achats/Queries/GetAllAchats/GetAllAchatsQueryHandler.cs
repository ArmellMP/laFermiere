using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Achats.Queries.GetAllAchats
{
    public class GetAllAchatsQueryHandler : IRequestHandler<GetAllAchatsQuery, List<AchatResponseDto>>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<GetAllAchatsQueryHandler> _logger;

        public GetAllAchatsQueryHandler(
            IAchatRepository achatRepository,
            ILogger<GetAllAchatsQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _logger = logger;
        }

        public async Task<List<AchatResponseDto>> Handle(
            GetAllAchatsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des achats");

            IEnumerable<Domain.Entities.Achat> achats;

            // Filtrer par date
            if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                achats = await _achatRepository.GetByDateRangeAsync(
                    request.DateDebut.Value,
                    request.DateFin.Value,
                    cancellationToken);
            }
            // Filtrer par bande
            else if (request.BandeId.HasValue)
            {
                achats = await _achatRepository.GetByBandeIdAsync(request.BandeId.Value, cancellationToken);
            }
            // Filtrer par fournisseur
            else if (request.FournisseurId.HasValue)
            {
                achats = await _achatRepository.GetByFournisseurIdAsync(request.FournisseurId.Value, cancellationToken);
            }
            // Uniquement les achats non reçus
            else if (request.OnlyNonRecus.HasValue && request.OnlyNonRecus.Value)
            {
                achats = await _achatRepository.GetNonRecusAsync(cancellationToken);
            }
            // Tous les achats
            else
            {
                achats = await _achatRepository.GetAllAsync(cancellationToken);
            }

            // Appliquer les filtres supplémentaires en mémoire
            if (!string.IsNullOrEmpty(request.TypeAchat))
            {
                achats = achats.Where(a => a.TypeAchat.ToString() == request.TypeAchat);
            }

            return achats.ToResponseDtos();
        }
    }
}