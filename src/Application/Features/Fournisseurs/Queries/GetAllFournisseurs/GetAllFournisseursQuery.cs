using MediatR;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Queries.GetAllFournisseurs
{
    public record GetAllFournisseursQuery : IRequest<List<FournisseurResponseDto>>
    {
        public string? TypeFournisseur { get; init; }
        public bool? OnlyActifs { get; init; }
        public string? SearchTerm { get; init; }
    }
}