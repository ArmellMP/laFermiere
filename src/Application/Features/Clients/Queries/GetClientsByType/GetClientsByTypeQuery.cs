using MediatR;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Queries.GetClientsByType
{
    /// <summary>
    /// Query pour récupérer les clients par type. Utile pour les statistiques et le ciblage commercial.
    /// </summary>
    public record GetClientsByTypeQuery : IRequest<List<ClientResponseDto>>
    {
        /// <summary>
        /// Type de client (Particulier, Restaurant, Grossiste, etc.)
        /// </summary>
        public string TypeClient { get; init; } = string.Empty;
        
        /// <summary>
        /// Uniquement les clients actifs
        /// </summary>
        public bool OnlyActifs { get; init; } = true;
    }
}