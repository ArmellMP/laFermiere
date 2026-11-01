using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepAchatEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public int FournisseurId { get; set; }
        public int BandeId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime DateAchat { get; set; }
        public string TypeAchat { get; set; } = string.Empty;      // string (SQL ENUM)
        public string? Description { get; set; }
        public decimal Quantite { get; set; }
        public string UniteMesure { get; set; } = string.Empty;    // string (SQL ENUM)
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        public string? FactureNumero { get; set; }
        public DateTime? DateReception { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS ====
        public string? FournisseurNom { get; set; }
        public string? BandeCode { get; set; }
        public string? CreateurNom { get; set; }

        /// <summary>
        /// Convertit RepAchatEntity en entité Domain Achat
        /// </summary>
        public Achat ToDomain()
        {
            // Conversion sécurisée du type d'achat
            TypeAchat typeAchat = Domain.Enums.TypeAchat.Autre;
            if (!string.IsNullOrEmpty(TypeAchat))
            {
                if (Enum.TryParse<TypeAchat>(TypeAchat, true, out var parsed))
                {
                    typeAchat = parsed;
                }
            }

            // Conversion sécurisée de l'unité de mesure
            UniteMesureAchat uniteMesure = Domain.Enums.UniteMesureAchat.Unite;
            if (!string.IsNullOrEmpty(UniteMesure))
            {
                if (Enum.TryParse<UniteMesureAchat>(UniteMesure, true, out var parsed))
                {
                    uniteMesure = parsed;
                }
            }

            return new Achat
            {
                Id = this.Id,
                FournisseurId = this.FournisseurId,
                BandeId = this.BandeId,
                CreatedBy = this.CreatedBy,
                DateAchat = this.DateAchat,
                TypeAchat = typeAchat,
                Description = this.Description,
                Quantite = this.Quantite,
                UniteMesure = uniteMesure,
                PrixUnitaire = this.PrixUnitaire,
                MontantTotal = this.MontantTotal,
                FactureNumero = this.FactureNumero,
                DateReception = this.DateReception,
                Observations = this.Observations,
                CreatedAt = this.CreatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Achat en RepAchatEntity
        /// </summary>
        public static RepAchatEntity FromDomain(Achat achat)
        {
            return new RepAchatEntity
            {
                Id = achat.Id,
                FournisseurId = achat.FournisseurId,
                BandeId = achat.BandeId,
                CreatedBy = achat.CreatedBy,
                DateAchat = achat.DateAchat,
                TypeAchat = achat.TypeAchat.ToString(),
                Description = achat.Description,
                Quantite = achat.Quantite,
                UniteMesure = achat.UniteMesure.ToString(),
                PrixUnitaire = achat.PrixUnitaire,
                MontantTotal = achat.MontantTotal,
                FactureNumero = achat.FactureNumero,
                DateReception = achat.DateReception,
                Observations = achat.Observations,
                CreatedAt = achat.CreatedAt
            };
        }

        public static implicit operator Achat?(RepAchatEntity? rep)
        {
            return rep?.ToDomain();
        }

        public static implicit operator RepAchatEntity?(Achat? achat)
        {
            return achat == null ? null : FromDomain(achat);
        }
    }
}