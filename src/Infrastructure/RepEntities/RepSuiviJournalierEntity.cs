using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepSuiviJournalierEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public int BandeId { get; set; }
        public int Jour { get; set; }
        public DateTime DateSuivi { get; set; }
        public int NbMortalites { get; set; }
        public string? CauseMortalite { get; set; }
        public decimal? PoidsMoyen { get; set; }
        public int? NbSujetsPeses { get; set; }
        public string? TypeSoin { get; set; }           // string (SQL ENUM)
        public string? ProduitUtilise { get; set; }
        public string? Dosage { get; set; }
        public int? DureeTraitement { get; set; }
        public decimal? CoutSoin { get; set; }
        public int? StockConsommeId { get; set; }
        public decimal? QuantiteConsommee { get; set; }
        public decimal? Temperature { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS (optionnel) ====
        public string? BandeCode { get; set; }
        public string? StockNom { get; set; }
        public string? StockUniteMesure { get; set; }
        public string? CreateurNom { get; set; }

        /// <summary>
        /// Convertit RepSuiviJournalierEntity en entité Domain SuiviJournalier. conversion string → enum.
        /// </summary>
        public SuiviJournalier ToDomain()
        {
            // Conversion sécurisée du type de soin
            TypeSoin? typeSoin = null;
            if (!string.IsNullOrEmpty(TypeSoin))
            {
                if (Enum.TryParse<TypeSoin>(TypeSoin, true, out var parsed))
                {
                    typeSoin = parsed;
                }
            }

            return new SuiviJournalier
            {
                Id = this.Id,
                BandeId = this.BandeId,
                Jour = this.Jour,
                DateSuivi = this.DateSuivi,
                NbMortalites = this.NbMortalites,
                CauseMortalite = this.CauseMortalite,
                PoidsMoyen = this.PoidsMoyen,
                NbSujetsPeses = this.NbSujetsPeses,
                TypeSoin = typeSoin,
                ProduitUtilise = this.ProduitUtilise,
                Dosage = this.Dosage,
                DureeTraitement = this.DureeTraitement,
                CoutSoin = this.CoutSoin,
                StockConsommeId = this.StockConsommeId,
                QuantiteConsommee = this.QuantiteConsommee,
                Temperature = this.Temperature,
                Observations = this.Observations,
                CreatedAt = this.CreatedAt,
                CreatedBy = this.CreatedBy,
                UpdatedAt = this.UpdatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain SuiviJournalier en RepSuiviJournalierEntity. conversion enum → string.
        /// </summary>
        public static RepSuiviJournalierEntity FromDomain(SuiviJournalier suivi)
        {
            return new RepSuiviJournalierEntity
            {
                Id = suivi.Id,
                BandeId = suivi.BandeId,
                Jour = suivi.Jour,
                DateSuivi = suivi.DateSuivi,
                NbMortalites = suivi.NbMortalites,
                CauseMortalite = suivi.CauseMortalite,
                PoidsMoyen = suivi.PoidsMoyen,
                NbSujetsPeses = suivi.NbSujetsPeses,
                TypeSoin = suivi.TypeSoin?.ToString(),
                ProduitUtilise = suivi.ProduitUtilise,
                Dosage = suivi.Dosage,
                DureeTraitement = suivi.DureeTraitement,
                CoutSoin = suivi.CoutSoin,
                StockConsommeId = suivi.StockConsommeId,
                QuantiteConsommee = suivi.QuantiteConsommee,
                Temperature = suivi.Temperature,
                Observations = suivi.Observations,
                CreatedAt = suivi.CreatedAt,
                CreatedBy = suivi.CreatedBy,
                UpdatedAt = suivi.UpdatedAt
            };
        }

        /// <summary>
        /// Conversion implicite vers l'entité Domain
        /// </summary>
        public static implicit operator SuiviJournalier?(RepSuiviJournalierEntity? rep)
        {
            return rep?.ToDomain();
        }

        /// <summary>
        /// Conversion implicite depuis l'entité Domain
        /// </summary>
        public static implicit operator RepSuiviJournalierEntity?(SuiviJournalier? suivi)
        {
            return suivi == null ? null : FromDomain(suivi);
        }
    }
}