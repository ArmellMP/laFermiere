// PresentationApi/Models/Requests/CreateBandeRequest.cs

using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Bandes
{
    /// <summary>
    /// Requête pour créer une nouvelle bande.
    /// Ce DTO est spécifique à l'API et contient des attributs de validation HTTP.
    /// </summary>
    public class CreateBandeRequest
    {
        /// <summary>
        /// Date d'arrivée des poussins.
        /// Format: YYYY-MM-DD
        /// </summary>
        [Required(ErrorMessage = "La date d'arrivée est requise")]
        [DataType(DataType.Date)]
        public DateTime DateArrivee { get; set; }

        /// <summary>
        /// Nombre initial de poussins.
        /// Doit être supérieur à 0.
        /// </summary>
        [Required(ErrorMessage = "Le nombre initial de poussins est requis")]
        [Range(1, 50000, ErrorMessage = "Le nombre de poussins doit être entre 1 et 50 000")]
        public int NbPoussinsInit { get; set; }

        /// <summary>
        /// Observations optionnelles.
        /// </summary>
        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent pas dépasser 2000 caractères")]
        public string? Observations { get; set; }

        /// <summary>
        /// Code personnalisé (optionnel).
        /// Format: YYYY-MM-DD-XXX (ex: 2024-01-24-001)
        /// </summary>
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}-\d{3}$", 
            ErrorMessage = "Le code doit être au format: AAAA-MM-JJ-001")]
        [MaxLength(50)]
        public string? Code { get; set; }
    }
}