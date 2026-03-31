using MediatR;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Queries.GetFournisseursByType
{
    public record GetFournisseursByTypeQuery : IRequest<List<FournisseurResponseDto>>
    {
        public string TypeFournisseur { get; init; } = string.Empty;
        public bool OnlyActifs { get; init; } = true;
    }
}