using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Utilisateurs
{
    public class AuthenticateRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string Password { get; set; } = string.Empty;
    }
}