// Application/Features/Bandes/Dtos/CreateBandeDto.cs
using System;

namespace Application.Features.Bandes.Dtos
{
    /// <summary>
    /// DTO pour la CRÉATION d'une bande.
    /// Ne contient QUE les propriétés nécessaires à la création.
    /// </summary>
    public class CreateBandeDto
    {
        /// <summary>
        /// Date d'arrivée des poussins (obligatoire).
        /// </summary>
        public DateTime DateArrivee { get; set; }

        /// <summary>
        /// Nombre initial de poussins (obligatoire).
        /// Doit être supérieur à 0.
        /// </summary>
        public int NbPoussinsInit { get; set; }

        /// <summary>
        /// Observations optionnelles.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Code personnalisé (optionnel).
        /// Si non fourni, le système génère un code automatique.
        /// </summary>
        public string? Code { get; set; }
    }
}