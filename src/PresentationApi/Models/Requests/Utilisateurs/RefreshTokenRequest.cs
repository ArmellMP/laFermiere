using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Utilisateurs
{
    /// <summary>Corps de POST /refresh et POST /logout.</summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Le refresh token est requis")]
        [MaxLength(200, ErrorMessage = "Refresh token invalide")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
