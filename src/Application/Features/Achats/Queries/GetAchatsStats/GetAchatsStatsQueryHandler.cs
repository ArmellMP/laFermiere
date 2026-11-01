using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;

namespace Application.Features.Achats.Queries.GetAchatsStats
{
    public class GetAchatsStatsQueryHandler : IRequestHandler<GetAchatsStatsQuery, AchatsStatsDto>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<GetAchatsStatsQueryHandler> _logger;

        public GetAchatsStatsQueryHandler(
            IAchatRepository achatRepository,
            ILogger<GetAchatsStatsQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _logger = logger;
        }

        public async Task<AchatsStatsDto> Handle(
            GetAchatsStatsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques des achats");

            IEnumerable<Domain.Entities.Achat> achats;

            // Filtrer par bande
            if (request.BandeId.HasValue)
            {
                achats = await _achatRepository.GetByBandeIdAsync(request.BandeId.Value, cancellationToken);
            }
            // Filtrer par fournisseur
            else if (request.FournisseurId.HasValue)
            {
                achats = await _achatRepository.GetByFournisseurIdAsync(request.FournisseurId.Value, cancellationToken);
            }
            // Filtrer par date
            else if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                achats = await _achatRepository.GetByDateRangeAsync(
                    request.DateDebut.Value,
                    request.DateFin.Value,
                    cancellationToken);
            }
            else
            {
                achats = await _achatRepository.GetAllAsync(cancellationToken);
            }

            var stats = new AchatsStatsDto
            {
                TotalAchats = achats.Count(),
                MontantTotal = achats.Sum(a => a.MontantTotal),
                AchatsRecus = achats.Count(a => a.DateReception.HasValue),
                AchatsNonRecus = achats.Count(a => !a.DateReception.HasValue),
                MontantRecus = achats.Where(a => a.DateReception.HasValue).Sum(a => a.MontantTotal),
                MontantNonRecus = achats.Where(a => !a.DateReception.HasValue).Sum(a => a.MontantTotal)
            };

            // Statistiques par type
            foreach (var achat in achats)
            {
                var typeKey = achat.TypeAchat.ToString();
                
                if (!stats.MontantParType.ContainsKey(typeKey))
                {
                    stats.MontantParType[typeKey] = 0;
                    stats.QuantiteParType[typeKey] = 0;
                }
                
                stats.MontantParType[typeKey] += achat.MontantTotal;
                stats.QuantiteParType[typeKey] += achat.Quantite;
            }

            // Statistiques par fournisseur (besoin d'un JOIN, sera fait en Infrastructure)
            // Pour l'instant, on laisse vide

            return stats;
        }
    }
}