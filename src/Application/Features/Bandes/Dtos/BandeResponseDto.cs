using Domain.Enums;

namespace Application.Features.Bandes.Dtos
{
    /// <summary>
    /// DTO pour toutes les réponses (GET, POST, PUT).
    /// </summary>
    public class BandeResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime DateArrivee { get; set; }
        public int NbPoussinsInit { get; set; }
        public int NbPoussinsActuel { get; set; }
        public StatutBande Statut { get; set; }
        public string StatutLibelle => GetStatutLibelle(Statut);
        public DateTime? DateCloture { get; set; }
        public string? Observations { get; set; }
        public decimal SuperficieCalculee { get; set; }
        public decimal TauxMortalite => NbPoussinsInit > 0 
            ? ((NbPoussinsInit - NbPoussinsActuel) / (decimal)NbPoussinsInit) * 100 
            : 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Optionnel : propriétés détaillées (pour les besoins spécifiques)
        public bool IncludeDetails { get; set; }
        public int? TotalMortalites => IncludeDetails ? NbPoussinsInit - NbPoussinsActuel : null;
        public int? JoursEcoules => IncludeDetails ? (DateTime.Now - DateArrivee).Days : null;
        public int? JoursRestants => IncludeDetails ? Math.Max(0, 45 - (DateTime.Now - DateArrivee).Days) : null;
        public decimal? PourcentageAvancement => IncludeDetails 
            ? Math.Min(100, (decimal)(DateTime.Now - DateArrivee).Days / 45 * 100)
            : null;
        
        private static string GetStatutLibelle(StatutBande statut)
        {
            return statut switch
            {
                StatutBande.EnPreparation => "En préparation",
                StatutBande.EnCours => "En cours",
                StatutBande.Cloturee => "Clôturée",
                StatutBande.Archivee => "Archivée",
                _ => "Inconnu"
            };
        }
    }
}