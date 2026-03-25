using Domain.Enums;

namespace Application.Features.SuivisJournaliers.Dtos
{
    /// <summary>
    /// DTO pour les réponses (quand on renvoie un suivi a l'utilisateur).
    /// </summary>
    public class SuiviResponseDto
    {
        public int Id { get; set; }
        public int BandeId { get; set; }
        public string? BandeCode { get; set; }       // Rempli via JOIN
        
        public int Jour { get; set; }
        public DateTime DateSuivi { get; set; }
        
        // ==== MORTALITÉS ====
        public int NbMortalites { get; set; }
        public string? CauseMortalite { get; set; }
        
        // ==== PESÉES ====
        public decimal? PoidsMoyen { get; set; }
        public int? NbSujetsPeses { get; set; }
        public bool PeseeEffectuee => PoidsMoyen.HasValue && NbSujetsPeses.HasValue;
        
        // ==== SOINS ====
        public string? TypeSoin { get; set; }
        public string? TypeSoinLibelle => GetTypeSoinLibelle(TypeSoin);
        public string? ProduitUtilise { get; set; }
        public string? Dosage { get; set; }
        public int? DureeTraitement { get; set; }
        public decimal? CoutSoin { get; set; }
        
        // ==== CONSOMMATION STOCK ====
        public int? StockConsommeId { get; set; }
        public string? StockConsommeNom { get; set; }       // Rempli via JOIN
        public decimal? QuantiteConsommee { get; set; }
        public string? UniteMesureStock { get; set; }       // Rempli via JOIN
        
        // ==== CONDITIONS ====
        public decimal? Temperature { get; set; }
        public string? TemperatureAvis => GetTemperatureAvis(Temperature);
        
        // ==== NOTES ====
        public string? Observations { get; set; }
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreateurNom { get; set; }  // Rempli via JOIN
        
        // ==== PROPRIÉTÉS CALCULÉES ====
        
        /// <summary>
        /// Âge des poussins au moment du suivi (en jours)
        /// </summary>
        public int AgeEnJours => (DateSuivi - DateArriveeBande).Days;
        
        // Note: DateArriveeBande sera rempli via JOIN ou calculé séparément
        public DateTime DateArriveeBande { get; set; }
        
        /// <summary>
        /// Convertit le code technique du type de soin en libellé français
        /// </summary>
        private string? GetTypeSoinLibelle(string? typeSoin)
        {
            return typeSoin switch
            {
                "Vaccination" => "Vaccination",
                "Traitement" => "Traitement",
                "Prevention" => "Prévention",
                "Supplement" => "Supplément",
                "Autre" => "Autre",
                _ => null
            };
        }
        
        /// <summary>
        /// Donne un avis sur la température
        /// </summary>
        private string? GetTemperatureAvis(decimal? temperature)
        {
            if (!temperature.HasValue) return null;
            
            return temperature.Value switch
            {
                < 18 => "Trop froid",
                >= 18 and <= 25 => "Idéal",
                > 25 and <= 30 => "Chaud",
                > 30 => "Trop chaud"
            };
        }
    }
}