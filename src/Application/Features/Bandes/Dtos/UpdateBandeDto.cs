// Application/Features/Bandes/Dtos/UpdateBandeDto.cs
using System;
using Domain.Enums;

namespace Application.Features.Bandes.Dtos
{
    /// <summary>
    /// DTO pour la MISE À JOUR d'une bande.
    /// Ne contient QUE les propriétés modifiables.
    /// </summary>
    public class UpdateBandeDto
    {
        /// <summary>
        /// Identifiant de la bande à modifier (obligatoire).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre actuel de poussins (obligatoire).
        /// </summary>
        public int NbPoussinsActuel { get; set; }

        /// <summary>
        /// Observations mises à jour (optionnel).
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Nouveau statut de la bande (obligatoire).
        /// </summary>
        public StatutBande Statut { get; set; }

        /// <summary>
        /// Date de clôture (optionnelle, à renseigner si Statut = Cloturee).
        /// </summary>
        public DateTime? DateCloture { get; set; }
    }
}