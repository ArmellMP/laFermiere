using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Clients
{
    /// <summary>
    /// Requête pour mettre à jour un client existant.
    /// </summary>
    public class UpdateClientRequest
    {
        [Required(ErrorMessage = "L'ID du client est requis")]
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-']+$", 
            ErrorMessage = "Le nom ne peut contenir que des lettres, espaces, tirets et apostrophes")]
        public string? Nom { get; set; }

        [RegularExpression("^(Particulier|Restaurant|Marche|Grossiste|Transformateur|Autre)$", 
            ErrorMessage = "Type de client invalide")]
        public string? TypeClient { get; set; }

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

        public bool? EstActif { get; set; }
    }
}