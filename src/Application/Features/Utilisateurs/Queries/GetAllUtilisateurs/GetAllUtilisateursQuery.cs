using MediatR;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Queries.GetAllUtilisateurs
{
    public record GetAllUtilisateursQuery : IRequest<List<UtilisateurResponseDto>>
    {
        public string? Role { get; init; }
        public bool? OnlyActifs { get; init; }
        public string? SearchTerm { get; init; }
    }
}