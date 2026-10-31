using MediatR;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Queries.GetAllParametres
{
    public record GetAllParametresQuery : IRequest<List<ParametreResponseDto>>
    {
        public string? Categorie { get; init; }
        public bool? OnlyModifiables { get; init; }
        public string? SearchTerm { get; init; }
    }
}