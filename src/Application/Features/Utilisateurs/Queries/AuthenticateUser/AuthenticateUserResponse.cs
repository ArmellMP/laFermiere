// ============================================
// FICHIER: Application/Features/Utilisateurs/Queries/AuthenticateUser/AuthenticateUserResponse.cs
// ============================================

using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Queries.AuthenticateUser
{
    public class AuthenticateUserResponse
    {
        public UtilisateurResponseDto Utilisateur { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }

        /// <summary>À présenter à /refresh pour obtenir un nouveau jeton sans mot de passe.</summary>
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshExpiration { get; set; }
    }
}