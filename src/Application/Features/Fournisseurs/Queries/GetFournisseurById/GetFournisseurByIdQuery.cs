using MediatR;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Queries.GetFournisseurById
{
    public record GetFournisseurByIdQuery : IRequest<FournisseurResponseDto>
    {
        public int Id { get; init; }
        public bool IncludeAchats { get; init; }
    }
}