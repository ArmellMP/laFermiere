using MediatR;

namespace Application.Features.SuivisJournaliers.Queries.GetSuiviStats
{
    /// <summary>
    /// Statistiques globales ou par bande des suivis journaliers.
    /// </summary>
    public record GetSuiviStatsQuery : IRequest<SuiviStatsDto>
    {
        /// <summary>
        /// Optionnel : Si spécifié, stats pour une bande uniquement
        /// </summary>
        public int? BandeId { get; init; }

        /// <summary>
        /// Optionnel : Période de début
        /// </summary>
        public DateTime? DateDebut { get; init; }

        /// <summary>
        /// Optionnel : Période de fin
        /// </summary>
        public DateTime? DateFin { get; init; }
    }

    /// <summary>
    /// DTO pour les statistiques des suivis
    /// </summary>
    public class SuiviStatsDto
    {
        public int TotalSuivis { get; set; }
        public int TotalMortalites { get; set; }
        public decimal? PoidsMoyenGlobal { get; set; }
        // Nullable comme PoidsMoyenGlobal : null = aucune température relevée sur la période
        // (0 °C serait une fausse valeur).
        public decimal? TemperatureMoyenne { get; set; }
        public int JoursAvecSoins { get; set; }
        public decimal? ConsommationMoyenneParJour { get; set; }
        
        // Statistiques par type de soin
        public int Vaccinations { get; set; }
        public int Traitements { get; set; }
        public int Preventions { get; set; }
        
        // Évolution
        public List<SuiviParJourDto> EvolutionQuotidienne { get; set; } = new();
    }

    public class SuiviParJourDto
    {
        public int Jour { get; set; }
        public DateTime Date { get; set; }
        public decimal? PoidsMoyen { get; set; }
        public int NbMortalites { get; set; }
        public decimal? Temperature { get; set; }
    }
}