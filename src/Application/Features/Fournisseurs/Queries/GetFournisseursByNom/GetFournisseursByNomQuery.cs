using MediatR;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Queries.GetFournisseursByNom
{
    public record GetFournisseursByNomQuery : IRequest<List<FournisseurResponseDto>>
    {
        public string SearchTerm { get; init; } = string.Empty;
        public int Limit { get; init; } = 10;
    }
}
