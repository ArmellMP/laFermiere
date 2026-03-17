using System;

using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepBandeEntities
{
    /// <summary>
    /// Entité Repository pour la table Bandes.
    /// </summary>
    public class RepBandeEntity
    {
        // ==== PROPRIÉTÉS DIRECTEMENT MAPPÉES DE LA TABLE ====
        
        /// <summary>
        /// ID de la bande (clé primaire).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code unique au format "ANNEE-MOIS-JOUR-NUM", Mappe exactement : Code VARCHAR(50)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Date d'arrivée des poussins, Mappe exactement : DateArrivee DATE
        /// </summary>
        public DateTime DateArrivee { get; set; }

        /// <summary>
        /// Nombre initial de poussins, Mappe exactement : NbPoussinsInit INT
        /// </summary>
        public int NbPoussinsInit { get; set; }

        /// <summary>
        /// Nombre actuel de poussins, Mappe exactement : NbPoussinsActuel INT
        /// </summary>
        public int NbPoussinsActuel { get; set; }

        /// <summary>
        /// Statut sous forme de string (comme en SQL).
        /// Mappe exactement : Statut ENUM('EnPreparation', 'EnCours', 'Cloturee', 'Archivee')
        /// IMPORTANT : C'est un string, pas un enum !
        /// </summary>
        public string Statut { get; set; } = string.Empty;

        /// <summary>
        /// Date de clôture (nullable), Mappe exactement : DateCloture DATE
        /// </summary>
        public DateTime? DateCloture { get; set; }

        /// <summary>
        /// Observations et notes, Mappe exactement : Observations TEXT
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Date de création, Mappe exactement : CreatedAt DATETIME
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date de dernière modification, Mappe exactement : UpdatedAt DATETIME
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Superficie calculée (colonne générée).
        /// Mappe exactement : SuperficieCalculee DECIMAL(8,2) GENERATED ALWAYS AS (NbPoussinsInit / 10.0) STORED
        /// IMPORTANT : En SQL c'est une colonne stockée, donc on la reçoit !
        /// </summary>
        public decimal SuperficieCalculee { get; set; }

        // ==== PROPRIÉTÉS ADDITIONNELLES POSSIBLES (pour les JOINs futurs) ====
        
        /// <summary>
        /// Pour les requêtes avec JOIN : nombre total de suivis journaliers.
        /// </summary>
        public int? TotalSuivis { get; set; }

        /// <summary>
        /// Pour les requêtes avec JOIN : nombre total de mortalités.
        /// </summary>
        public int? TotalMortalites { get; set; }

        /// <summary>
        /// Pour les requêtes avec JOIN : poids moyen actuel.
        /// </summary>
        public decimal? PoidsMoyenActuel { get; set; }

        // ==== MÉTHODES DE CONVERSION ====

        /// <summary>
        /// Convertit RepBandeEntity en entité Domain Bande, C'est ici qu'on fait la conversion string → enum.
        /// </summary>
        /// <returns>Une instance de Bande (Domain Entity)</returns>
        public Bande ToDomain()
        {
            return new Bande
            {
                Id = this.Id,
                Code = this.Code,
                DateArrivee = this.DateArrivee,
                NbPoussinsInit = this.NbPoussinsInit,
                NbPoussinsActuel = this.NbPoussinsActuel,
                // CONVERSION CRITIQUE : string SQL → enum C#
                Statut = ConvertStringToStatutBande(this.Statut),
                DateCloture = this.DateCloture,
                Observations = this.Observations,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
                // NOTE : SuperficieCalculee n'est pas settée car elle est calculée dans l'entité
            };
        }

        /// <summary>
        /// Convertit une entité Domain Bande en RepBandeEntity.
        /// Pour les INSERT/UPDATE.
        /// </summary>
        /// <param name="bande">L'entité Domain Bande</param>
        /// <returns>Une instance de RepBandeEntity</returns>
        public static RepBandeEntity FromDomain(Bande bande)
        {
            return new RepBandeEntity
            {
                Id = bande.Id,
                Code = bande.Code,
                DateArrivee = bande.DateArrivee,
                NbPoussinsInit = bande.NbPoussinsInit,
                NbPoussinsActuel = bande.NbPoussinsActuel,
                // CONVERSION CRITIQUE : enum C# → string SQL
                Statut = bande.Statut.ToString(),
                DateCloture = bande.DateCloture,
                Observations = bande.Observations,
                CreatedAt = bande.CreatedAt,
                UpdatedAt = bande.UpdatedAt,
                // On recalcule ou on prend depuis SQL
                SuperficieCalculee = bande.NbPoussinsInit / 10.0m
            };
        }

        /// <summary>
        /// Méthode helper pour convertir string → StatutBande.
        /// Gère les erreurs de conversion.
        /// </summary>
        private static StatutBande ConvertStringToStatutBande(string statutString)
        {
            if (string.IsNullOrEmpty(statutString))
                return StatutBande.EnPreparation;

            return Enum.TryParse<StatutBande>(statutString, true, out var statut)
                ? statut
                : StatutBande.EnPreparation; // Valeur par défaut en cas d'erreur
        }

        /// <summary>
        /// Version simplifiée pour les cas simples.
        /// </summary>
        public static implicit operator Bande?(RepBandeEntity? repBande)
        {
            return repBande?.ToDomain();
        }

        /// <summary>
        /// Version simplifiée pour la conversion inverse.
        /// </summary>
        public static implicit operator RepBandeEntity?(Bande? bande)
        {
            return bande == null ? null : FromDomain(bande);
        }
    }
}