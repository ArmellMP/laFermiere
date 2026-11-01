using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Utilisateurs
{
    public class CreateUtilisateurRequest
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        [MaxLength(100, ErrorMessage = "L'email ne peut dépasser 100 caractères")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [MaxLength(100, ErrorMessage = "Le mot de passe ne peut dépasser 100 caractères")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est requis")]
        [RegularExpression("^(Admin|Fermier|Veterinaire|Comptable)$", 
            ErrorMessage = "Rôle invalide. Valeurs acceptées : Admin, Fermier, Veterinaire, Comptable")]
        public string Role { get; set; } = string.Empty;
    }
}