using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Ventes.Interfaces;

namespace Application.Features.Ventes.Queries.GetVentesStats
{
    public class GetVentesStatsQueryHandler : IRequestHandler<GetVentesStatsQuery, VentesStatsDto>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly ILogger<GetVentesStatsQueryHandler> _logger;

        public GetVentesStatsQueryHandler(
            IVenteRepository venteRepository,
            ILogger<GetVentesStatsQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _logger = logger;
        }

        public async Task<VentesStatsDto> Handle(
            GetVentesStatsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques des ventes");

            IEnumerable<Domain.Entities.Vente> ventes;

            // Filtrer par bande
            if (request.BandeId.HasValue)
            {
                ventes = await _venteRepository.GetByBandeIdAsync(request.BandeId.Value, cancellationToken);
            }
            // Filtrer par date
            else if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                ventes = await _venteRepository.GetByDateRangeAsync(
                    request.DateDebut.Value, 
                    request.DateFin.Value, 
                    cancellationToken);
            }
            else
            {
                ventes = await _venteRepository.GetAllAsync(cancellationToken);
            }

            var stats = new VentesStatsDto
            {
                TotalVentes = ventes.Count(),
                MontantTotal = ventes.Sum(v => v.MontantTotal),
                MontantPaye = ventes.Where(v => v.EstPaye).Sum(v => v.MontantTotal),
                MontantImpaye = ventes.Where(v => !v.EstPaye).Sum(v => v.MontantTotal),
                NombreVentesPayees = ventes.Count(v => v.EstPaye),
                NombreVentesImpayees = ventes.Count(v => !v.EstPaye)
            };

            // Statistiques par type de vente
            foreach (var vente in ventes)
            {
                var typeKey = vente.TypeVente.ToString();
                
                if (!stats.MontantParType.ContainsKey(typeKey))
                {
                    stats.MontantParType[typeKey] = 0;
                    stats.QuantiteParType[typeKey] = 0;
                }
                
                stats.MontantParType[typeKey] += vente.MontantTotal;
                stats.QuantiteParType[typeKey] += (int)vente.Quantite;
            }

            // Statistiques par mode de paiement
            foreach (var vente in ventes)
            {
                var modeKey = vente.ModePaiement.ToString();
                
                if (!stats.MontantParModePaiement.ContainsKey(modeKey))
                {
                    stats.MontantParModePaiement[modeKey] = 0;
                }
                
                stats.MontantParModePaiement[modeKey] += vente.MontantTotal;
            }

            return stats;
        }
    }
}