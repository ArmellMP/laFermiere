using MediatR;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Queries.GetClientById
{
    /// <summary>
    /// Query pour récupérer un client par son ID.
    /// </summary>
    public record GetClientByIdQuery : IRequest<ClientResponseDto>
    {
        public int Id { get; init; }
        
        /// <summary>
        /// Inclure les informations de ventes (optionnel)
        /// </summary>
        public bool IncludeVentes { get; init; }
    }
}