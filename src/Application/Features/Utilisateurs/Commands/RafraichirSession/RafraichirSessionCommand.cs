using Application.Features.Utilisateurs.Queries.AuthenticateUser;
using MediatR;

namespace Application.Features.Utilisateurs.Commands.RafraichirSession
{
    /// <summary>
    /// Échange un refresh token contre un nouveau jeton d'accès et un nouveau refresh token
    /// (rotation). Lève UnauthorizedAccessException si le token n'est plus valable.
    /// </summary>
    public record RafraichirSessionCommand : IRequest<AuthenticateUserResponse>
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
