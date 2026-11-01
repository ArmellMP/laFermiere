// ============================================
// FICHIER: Application/Features/Utilisateurs/Queries/AuthenticateUser/AuthenticateUserQuery.cs
// ============================================

using MediatR;

namespace Application.Features.Utilisateurs.Queries.AuthenticateUser
{
    /// <summary>
    /// Requête pour authentifier un utilisateur
    /// </summary>
    public record AuthenticateUserQuery : IRequest<AuthenticateUserResponse>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}