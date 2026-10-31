using MediatR;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Commands.CreateUtilisateur
{
    public record CreateUtilisateurCommand : IRequest<UtilisateurResponseDto>
    {
        public CreateUtilisateurDto CreateUtilisateurDto { get; init; } = null!;
    }
}