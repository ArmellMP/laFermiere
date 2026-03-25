using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.Bandes.Interfaces;

namespace Application.Features.SuivisJournaliers.Queries.GetSuiviStats
{
    /// <summary>
    /// Handler pour les statistiques des suivis. Afficher les données pour le dashboard.
    /// </summary>
    public class GetSuiviStatsQueryHandler : IRequestHandler<GetSuiviStatsQuery, SuiviStatsDto>
    {
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly ILogger<GetSuiviStatsQueryHandler> _logger;

        public GetSuiviStatsQueryHandler(
            ISuiviJournalierRepository suiviRepository,
            IBandeRepository bandeRepository,
            ILogger<GetSuiviStatsQueryHandler> logger)
        {
            _suiviRepository = suiviRepository;
            _bandeRepository = bandeRepository;
            _logger = logger;
        }

        public async Task<SuiviStatsDto> Handle(
            GetSuiviStatsQuery request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques des suivis");

            var stats = new SuiviStatsDto();
            IEnumerable<Domain.Entities.SuiviJournalier> suivis;

            // ============================================
            // 1. Récupérer les données selon les filtres
            // ============================================
            if (request.BandeId.HasValue)
            {
                // Vérifier que la bande existe
                var bande = await _bandeRepository.GetByIdAsync(request.BandeId.Value, cancellationToken);
                if (bande == null)
                {
                    return stats; // Retourne stats vides si bande inexistante
                }

                if (request.DateDebut.HasValue && request.DateFin.HasValue)
                {
                    suivis = await _suiviRepository.GetByBandeIdAndDateRangeAsync(
                        request.BandeId.Value,
                        request.DateDebut.Value,
                        request.DateFin.Value,
                        cancellationToken);
                }
                else
                {
                    suivis = await _suiviRepository.GetByBandeIdAsync(
                        request.BandeId.Value, 
                        cancellationToken);
                }
            }
            else
            {
                // Pour l'instant, on ne peut pas récupérer tous les suivis
                // (pas de méthode GetAll dans le repository)
                suivis = new List<Domain.Entities.SuiviJournalier>();
            }

            // ============================================
            // 2. Calculer les statistiques
            // ============================================
            var listeSuivis = suivis.ToList();
            
            stats.TotalSuivis = listeSuivis.Count;
            stats.TotalMortalites = listeSuivis.Sum(s => s.NbMortalites);
            stats.PoidsMoyenGlobal = listeSuivis
                .Where(s => s.PoidsMoyen.HasValue)
                .Average(s => s.PoidsMoyen);
            stats.TemperatureMoyenne = listeSuivis
                .Where(s => s.Temperature.HasValue)
                .Average(s => s.Temperature ?? 0);
            
            stats.JoursAvecSoins = listeSuivis.Count(s => s.TypeSoin.HasValue);
            stats.Vaccinations = listeSuivis.Count(s => s.TypeSoin == Domain.Enums.TypeSoin.Vaccination);
            stats.Traitements = listeSuivis.Count(s => s.TypeSoin == Domain.Enums.TypeSoin.Traitement);
            stats.Preventions = listeSuivis.Count(s => s.TypeSoin == Domain.Enums.TypeSoin.Prevention);

            // ============================================
            // 3. Évolution quotidienne
            // ============================================
            stats.EvolutionQuotidienne = listeSuivis
                .OrderBy(s => s.Jour)
                .Select(s => new SuiviParJourDto
                {
                    Jour = s.Jour,
                    Date = s.DateSuivi,
                    PoidsMoyen = s.PoidsMoyen,
                    NbMortalites = s.NbMortalites,
                    Temperature = s.Temperature
                })
                .ToList();

            return stats;
        }
    }
}