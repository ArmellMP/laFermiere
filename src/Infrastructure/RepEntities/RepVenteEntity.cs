using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepVenteEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public int BandeId { get; set; }
        public int ClientId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime DateVente { get; set; }
        public string TypeVente { get; set; } = string.Empty;      // string (SQL ENUM)
        public decimal Quantite { get; set; }
        public string UniteMesure { get; set; } = string.Empty;    // string (SQL ENUM)
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        public string ModePaiement { get; set; } = string.Empty;   // string (SQL ENUM)
        public bool EstPaye { get; set; }
        public DateTime? DatePaiement { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS ====
        public string? BandeCode { get; set; }
        public string? ClientNom { get; set; }
        public string? CreateurNom { get; set; }

        /// <summary>
        /// Convertit RepVenteEntity en entité Domain Vente
        /// Gère la conversion string → enum
        /// </summary>
        public Vente ToDomain()
        {
            // Conversion sécurisée du type de vente
            TypeVente typeVente = Domain.Enums.TypeVente.Autres;
            if (!string.IsNullOrEmpty(TypeVente))
            {
                if (Enum.TryParse<TypeVente>(TypeVente, true, out var parsed))
                {
                    typeVente = parsed;
                }
            }

            // Conversion sécurisée de l'unité de mesure
            UniteMesureVente uniteMesure = Domain.Enums.UniteMesureVente.Unite;
            if (!string.IsNullOrEmpty(UniteMesure))
            {
                if (Enum.TryParse<UniteMesureVente>(UniteMesure, true, out var parsed))
                {
                    uniteMesure = parsed;
                }
            }

            // Conversion sécurisée du mode de paiement
            ModePaiement modePaiement = Domain.Enums.ModePaiement.Especes;
            if (!string.IsNullOrEmpty(ModePaiement))
            {
                if (Enum.TryParse<ModePaiement>(ModePaiement, true, out var parsed))
                {
                    modePaiement = parsed;
                }
            }

            return new Vente
            {
                Id = this.Id,
                BandeId = this.BandeId,
                ClientId = this.ClientId,
                CreatedBy = this.CreatedBy,
                DateVente = this.DateVente,
                TypeVente = typeVente,
                Quantite = this.Quantite,
                UniteMesure = uniteMesure,
                PrixUnitaire = this.PrixUnitaire,
                MontantTotal = this.MontantTotal,
                ModePaiement = modePaiement,
                EstPaye = this.EstPaye,
                DatePaiement = this.DatePaiement,
                Observations = this.Observations,
                CreatedAt = this.CreatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Vente en RepVenteEntity
        /// Gère la conversion enum → string
        /// </summary>
        public static RepVenteEntity FromDomain(Vente vente)
        {
            return new RepVenteEntity
            {
                Id = vente.Id,
                BandeId = vente.BandeId,
                ClientId = vente.ClientId,
                CreatedBy = vente.CreatedBy,
                DateVente = vente.DateVente,
                TypeVente = vente.TypeVente.ToString(),
                Quantite = vente.Quantite,
                UniteMesure = vente.UniteMesure.ToString(),
                PrixUnitaire = vente.PrixUnitaire,
                MontantTotal = vente.MontantTotal,
                ModePaiement = vente.ModePaiement.ToString(),
                EstPaye = vente.EstPaye,
                DatePaiement = vente.DatePaiement,
                Observations = vente.Observations,
                CreatedAt = vente.CreatedAt
            };
        }

        public static implicit operator Vente?(RepVenteEntity? rep)
        {
            return rep?.ToDomain();
        }

        public static implicit operator RepVenteEntity?(Vente? vente)
        {
            return vente == null ? null : FromDomain(vente);
        }
    }
}