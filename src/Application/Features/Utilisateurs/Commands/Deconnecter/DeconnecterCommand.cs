using MediatR;

namespace Application.Features.Utilisateurs.Commands.Deconnecter
{
    /// <summary>Déconnexion : révoque la session à laquelle appartient ce refresh token.</summary>
    public record DeconnecterCommand : IRequest
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
