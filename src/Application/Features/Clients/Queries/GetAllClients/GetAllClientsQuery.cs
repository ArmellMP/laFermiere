using MediatR;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Queries.GetAllClients
{
    /// <summary>
    /// Query pour récupérer tous les clients.
    /// </summary>
    public record GetAllClientsQuery : IRequest<List<ClientResponseDto>>
    {
        /// <summary>
        /// Filtrer par type de client (optionnel)
        /// </summary>
        public string? TypeClient { get; init; }
        
        /// <summary>
        /// Filtrer par statut actif (optionnel)
        /// </summary>
        public bool? OnlyActifs { get; init; }
        
        /// <summary>
        /// Recherche par nom (optionnel)
        /// </summary>
        public string? SearchTerm { get; init; }
    }
}