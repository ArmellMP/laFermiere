using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Alertes.Interfaces;

namespace Application.Features.Alertes.Queries.GetAlertesStats
{
    public class GetAlertesStatsQueryHandler : IRequestHandler<GetAlertesStatsQuery, AlertesStatsDto>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<GetAlertesStatsQueryHandler> _logger;

        public GetAlertesStatsQueryHandler(
            IAlerteRepository alerteRepository,
            ILogger<GetAlertesStatsQueryHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _logger = logger;
        }

        public async Task<AlertesStatsDto> Handle(
            GetAlertesStatsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques des alertes");

            IEnumerable<Domain.Entities.Alerte> alertes;

            if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                alertes = await _alerteRepository.GetByDateRangeAsync(
                    request.DateDebut.Value,
                    request.DateFin.Value,
                    cancellationToken);
            }
            else
            {
                alertes = await _alerteRepository.GetAllAsync(cancellationToken);
            }

            var stats = new AlertesStatsDto
            {
                TotalAlertes = alertes.Count(),
                AlertesNonLues = alertes.Count(a => !a.EstLue),
                AlertesLues = alertes.Count(a => a.EstLue)
            };

            // Statistiques par type
            foreach (var alerte in alertes)
            {
                var typeKey = alerte.TypeAlerte.ToString();
                if (!stats.CountParType.ContainsKey(typeKey))
                    stats.CountParType[typeKey] = 0;
                stats.CountParType[typeKey]++;
            }

            // Statistiques par niveau
            foreach (var alerte in alertes)
            {
                var niveauKey = alerte.Niveau.ToString();
                if (!stats.CountParNiveau.ContainsKey(niveauKey))
                    stats.CountParNiveau[niveauKey] = 0;
                stats.CountParNiveau[niveauKey]++;
            }

            // Évolution par jour (pour les 30 derniers jours)
            var dateLimite = DateTime.UtcNow.AddDays(-30);
            var alertesRecentes = alertes.Where(a => a.DateAlerte >= dateLimite);
            
            foreach (var alerte in alertesRecentes)
            {
                var jourKey = alerte.DateAlerte.ToString("yyyy-MM-dd");
                if (!stats.EvolutionParJour.ContainsKey(jourKey))
                    stats.EvolutionParJour[jourKey] = 0;
                stats.EvolutionParJour[jourKey]++;
            }

            return stats;
        }
    }
}