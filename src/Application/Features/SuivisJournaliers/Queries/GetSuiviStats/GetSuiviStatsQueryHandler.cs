using Application.Common.Exceptions;
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

            // ============================================
            // 1. Vérifier la bande demandée (si un filtre de bande est fourni)
            // ============================================
            // Une bande inexistante est une erreur de la requête : NotFoundException => HTTP 404,
            // comme partout ailleurs dans l'API (avant : des statistiques vides étaient renvoyées,
            // ce qui ne permettait pas de distinguer "bande inconnue" de "bande sans suivi").
            if (request.BandeId.HasValue)
            {
                var bande = await _bandeRepository.GetByIdAsync(request.BandeId.Value, cancellationToken);
                if (bande == null)
                {
                    throw new NotFoundException($"Bande avec l'ID {request.BandeId.Value} non trouvée.");
                }
            }

            // ============================================
            // 2. Récupérer les suivis selon les filtres (tous facultatifs)
            // ============================================
            // Sans bandeId : statistiques globales sur toutes les bandes.
            // DateDebut et DateFin s'appliquent même si une seule des deux est fournie.
            var listeSuivis = (await _suiviRepository.GetByFiltresAsync(
                request.BandeId,
                request.DateDebut,
                request.DateFin,
                cancellationToken)).ToList();

            // ============================================
            // 3. Calculer les statistiques
            // ============================================
            stats.TotalSuivis = listeSuivis.Count;
            stats.TotalMortalites = listeSuivis.Sum(s => s.NbMortalites);

            // Moyennes : on fait la moyenne de valeurs NULLABLES (decimal?).
            // LINQ ignore alors les null et renvoie null si aucune valeur n'existe.
            // (Avant : ".Average(s => s.Temperature ?? 0)" portait sur des decimal non nullables,
            // et Average() sur une liste vide lève "Sequence contains no elements" => erreur 500.)
            stats.PoidsMoyenGlobal = listeSuivis.Average(s => s.PoidsMoyen);
            stats.TemperatureMoyenne = listeSuivis.Average(s => s.Temperature);
            
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