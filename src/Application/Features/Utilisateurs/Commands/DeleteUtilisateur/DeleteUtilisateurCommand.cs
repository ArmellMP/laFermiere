using MediatR;

namespace Application.Features.Utilisateurs.Commands.DeleteUtilisateur
{
    public record DeleteUtilisateurCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}