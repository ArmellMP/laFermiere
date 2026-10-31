using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Utilisateurs
{
    public class UpdateUtilisateurRequest
    {
        [Required(ErrorMessage = "L'ID de l'utilisateur est requis")]
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères")]
        public string? Nom { get; set; }

        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        [MaxLength(100, ErrorMessage = "L'email ne peut dépasser 100 caractères")]
        public string? Email { get; set; }

        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [MaxLength(100, ErrorMessage = "Le mot de passe ne peut dépasser 100 caractères")]
        public string? Password { get; set; }

        [RegularExpression("^(Admin|Fermier|Veterinaire|Comptable)$", 
            ErrorMessage = "Rôle invalide")]
        public string? Role { get; set; }

        public bool? EstActif { get; set; }
    }
}