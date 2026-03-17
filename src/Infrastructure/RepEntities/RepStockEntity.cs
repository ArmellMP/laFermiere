// Infrastructure/RepEntities/RepStockEntity.cs
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    /// <summary>
    /// Entité Repository pour la table Stocks.
    /// Représentation EXACTE de la table SQL, utilisée par Dapper.
    /// </summary>
    public class RepStockEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public int BandeId { get; set; }
        public string Type { get; set; } = string.Empty;  // string, pas enum (car SQL)
        public string Nom { get; set; } = string.Empty;
        public decimal QuantiteInitiale { get; set; }
        public decimal QuantiteActuelle { get; set; }
        public string UniteMesure { get; set; } = string.Empty;  // string, pas enum
        public decimal? SeuilAlerte { get; set; }
        public DateTime? DatePeremption { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public int? FournisseurId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS (optionnel) ====
        public string? BandeCode { get; set; }      // Pour JOIN avec Bandes
        public string? FournisseurNom { get; set; } // Pour JOIN avec Fournisseurs

        /// <summary>
        /// Convertit RepStockEntity en entité Domain Stock.
        /// Gère la conversion string → enum.
        /// </summary>
        public Stock ToDomain()
        {
            // Conversion sécurisée des enums
            if (!Enum.TryParse<TypeStock>(Type, true, out var typeStock))
            {
                typeStock = TypeStock.Autre; // Valeur par défaut
            }

            if (!Enum.TryParse<UniteMesure>(UniteMesure, true, out var uniteMesure))
            {
                uniteMesure = Domain.Enums.UniteMesure.Unite; // Valeur par défaut
            }

            return new Stock
            {
                Id = this.Id,
                BandeId = this.BandeId,
                Type = typeStock,
                Nom = this.Nom,
                QuantiteInitiale = this.QuantiteInitiale,
                QuantiteActuelle = this.QuantiteActuelle,
                UniteMesure = uniteMesure,
                SeuilAlerte = this.SeuilAlerte,
                DatePeremption = this.DatePeremption,
                PrixUnitaire = this.PrixUnitaire,
                FournisseurId = this.FournisseurId,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Stock en RepStockEntity.
        /// Gère la conversion enum → string.
        /// </summary>
        public static RepStockEntity FromDomain(Stock stock)
        {
            return new RepStockEntity
            {
                Id = stock.Id,
                BandeId = stock.BandeId,
                Type = stock.Type.ToString(),
                Nom = stock.Nom,
                QuantiteInitiale = stock.QuantiteInitiale,
                QuantiteActuelle = stock.QuantiteActuelle,
                UniteMesure = stock.UniteMesure.ToString(),
                SeuilAlerte = stock.SeuilAlerte,
                DatePeremption = stock.DatePeremption,
                PrixUnitaire = stock.PrixUnitaire,
                FournisseurId = stock.FournisseurId,
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };
        }

        /// <summary>
        /// Conversion implicite pour simplifier l'utilisation
        /// </summary>
        public static implicit operator Stock?(RepStockEntity? rep)
        {
            return rep?.ToDomain();
        }

        /// <summary>
        /// Conversion implicite pour simplifier l'utilisation
        /// </summary>
        public static implicit operator RepStockEntity?(Stock? stock)
        {
            return stock == null ? null : FromDomain(stock);
        }
    }
}