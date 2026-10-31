using MediatR;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Commands.UpdateUtilisateur
{
    public record UpdateUtilisateurCommand : IRequest<UtilisateurResponseDto>
    {
        public UpdateUtilisateurDto UpdateUtilisateurDto { get; init; } = null!;
    }
}