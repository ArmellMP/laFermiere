using MediatR;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Queries.GetUtilisateurById
{
    public record GetUtilisateurByIdQuery : IRequest<UtilisateurResponseDto>
    {
        public int Id { get; init; }
    }
}