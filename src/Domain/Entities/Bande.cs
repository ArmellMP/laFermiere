using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class Bande
    {
         public int Id { get; set; }

        /// <summary>
        /// Code unique de la bande au format "ANNEE-MOIS-JOUR-NUM".
        /// Exemple : "2024-01-08-001" pour la première bande du 8 janvier 2024.
        /// Correspond à "Code VARCHAR(50) UNIQUE NOT NULL" en SQL.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Date d'arrivée des poussins dans la bande.
        /// Correspond à "DateArrivee DATE NOT NULL" en SQL.
        /// </summary>
        public DateTime DateArrivee { get; set; }

        /// <summary>
        /// Nombre initial de poussins dans la bande.
        /// Doit être supérieur à 0.
        /// Correspond à "NbPoussinsInit INT NOT NULL" en SQL.
        /// </summary>
        public int NbPoussinsInit { get; set; }

        /// <summary>
        /// Nombre actuel de poussins dans la bande.
        /// Diminue avec les mortalités.
        /// Correspond à "NbPoussinsActuel INT NOT NULL DEFAULT 0" en SQL.
        /// </summary>
        public int NbPoussinsActuel { get; set; }

        /// <summary>
        /// Statut actuel de la bande dans son cycle de vie.
        /// Utilise l'enum StatutBande.
        /// Correspond à "Statut ENUM(...) NOT NULL DEFAULT 'EnPreparation'" en SQL.
        /// </summary>
        public StatutBande Statut { get; set; }

        /// <summary>
        /// Date de clôture de la bande (nullable).
        /// Rempli uniquement quand la bande est clôturée.
        /// Correspond à "DateCloture DATE" en SQL.
        /// </summary>
        public DateTime? DateCloture { get; set; }

        /// <summary>
        /// Observations et notes sur la bande.
        /// Correspond à "Observations TEXT" en SQL.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Date et heure de création de l'enregistrement.
        /// Correspond à "CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP" en SQL.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date et heure de dernière modification.
        /// Correspond à "UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP" en SQL.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Superficie calculée nécessaire pour la bande.
        /// Calculée comme NbPoussinsInit / 10.0 (10 poussins par m²).
        /// Correspond à "SuperficieCalculee DECIMAL(8,2) GENERATED ALWAYS AS (NbPoussinsInit / 10.0) STORED" en SQL.
        /// </summary>
        public decimal SuperficieCalculee => NbPoussinsInit / 10.0m;
    }
}
