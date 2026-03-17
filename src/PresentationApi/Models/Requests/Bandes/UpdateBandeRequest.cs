// PresentationAPI/Models/Requests/UpdateBandeRequest.cs

using System.ComponentModel.DataAnnotations;

namespace PresentationAPI.Models.Requests.Bandes
{
    /// <summary>
    /// Requête pour mettre à jour une bande existante.
    /// </summary>
    public class UpdateBandeRequest
    {
        /// <summary>
        /// Identifiant unique de la bande.
        /// </summary>
        [Required(ErrorMessage = "L'ID de la bande est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID doit être un nombre positif")]
        public int Id { get; set; }

        /// <summary>
        /// Nombre actuel de poussins dans la bande.
        /// </summary>
        [Required(ErrorMessage = "Le nombre actuel de poussins est requis")]
        [Range(0, 50000, ErrorMessage = "Le nombre de poussins doit être entre 0 et 50 000")]
        public int NbPoussinsActuel { get; set; }

        /// <summary>
        /// Observations mises à jour.
        /// </summary>
        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent pas dépasser 2000 caractères")]
        public string? Observations { get; set; }

        /// <summary>
        /// Statut de la bande.
        /// </summary>
        [Required(ErrorMessage = "Le statut est requis")]
        public string Statut { get; set; } = string.Empty;

        /// <summary>
        /// Date de clôture (si la bande est clôturée).
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? DateCloture { get; set; }
    }
}