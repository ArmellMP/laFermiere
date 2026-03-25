// Domain/Entities/SuiviJournalier.cs
using Domain.Enums;

namespace Domain.Entities
{
    public class SuiviJournalier
    {
        // ==== PROPRIÉTÉS DE BASE ====
        public int Id { get; set; }
        public int BandeId { get; set; }
        public int Jour { get; set; }
        public DateTime DateSuivi { get; set; }

        // ==== MORTALITÉS ====
        public int NbMortalites { get; set; }
        public string? CauseMortalite { get; set; }

        // ==== PESÉES ====
        public decimal? PoidsMoyen { get; set; }
        public int? NbSujetsPeses { get; set; }

        // ==== SOINS ====
        public TypeSoin? TypeSoin { get; set; }
        public string? ProduitUtilise { get; set; }
        public string? Dosage { get; set; }
        public int? DureeTraitement { get; set; }
        public decimal? CoutSoin { get; set; }

        // ==== CONSOMMATION STOCK ====
        public int? StockConsommeId { get; set; }
        public decimal? QuantiteConsommee { get; set; }

        // ==== CONDITIONS ====
        public decimal? Temperature { get; set; }

        // ==== NOTES ====
        public string? Observations { get; set; }

        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ============================================
        // PROPRIÉTÉS CALCULÉES (SANS DÉPENDANCE EXTERNE)
        // ============================================

        /// <summary>
        /// Vérifie si une pesée a été effectuée aujourd'hui
        /// </summary>
        public bool PeseeEffectuee => PoidsMoyen.HasValue && NbSujetsPeses.HasValue;

        /// <summary>
        /// Indique si un soin a été administré
        /// </summary>
        public bool SoinEffectue => TypeSoin.HasValue;

        /// <summary>
        /// Indique si du stock a été consommé
        /// </summary>
        public bool StockConsomme => StockConsommeId.HasValue && QuantiteConsommee.HasValue;

        /// <summary>
        /// Indique si la température est anormale
        /// (ne dépend que de la température elle-même)
        /// </summary>
        public bool TemperatureAnormale => Temperature.HasValue && 
            (Temperature < 18 || Temperature > 30);

        // ============================================
        // PROPRIÉTÉS DE NAVIGATION (commentées pour l'instant)
        // Seront implémentées quand on créera les entités liées
        // ============================================
        
        // public virtual Bande? Bande { get; set; }
        // public virtual Stock? StockConsomme { get; set; }
        // public virtual Utilisateur? Createur { get; set; }
    }
}