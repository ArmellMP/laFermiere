using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Clients
{
    /// <summary>
    /// Requête pour créer un nouveau client.
    /// </summary>
    public class CreateClientRequest
    {
        [Required(ErrorMessage = "Le nom du client est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-']+$", 
            ErrorMessage = "Le nom ne peut contenir que des lettres, espaces, tirets et apostrophes")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de client est requis")]
        [RegularExpression("^(Particulier|Restaurant|Marche|Grossiste|Transformateur|Autre)$", 
            ErrorMessage = "Type de client invalide. Valeurs acceptées : Particulier, Restaurant, Marche, Grossiste, Transformateur, Autre")]
        public string TypeClient { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        [RegularExpression(@"^[0-9+\-\s]+$", 
            ErrorMessage = "Le téléphone ne peut contenir que des chiffres, +, - et espaces")]
        public string? Telephone { get; set; }

        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide")]
        [MaxLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        public string? Email { get; set; }

        [MaxLength(500, ErrorMessage = "L'adresse ne peut pas dépasser 500 caractères")]
        public string? Adresse { get; set; }

        [MaxLength(2000, ErrorMessage = "Les notes ne peuvent pas dépasser 2000 caractères")]
        public string? Notes { get; set; }
    }
}