using MediatR;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Queries.GetClientsByNom
{
    /// <summary>
    /// Query pour rechercher des clients par nom. Utile pour l'autocomplétion dans les formulaires.
    /// </summary>
    public record GetClientsByNomQuery : IRequest<List<ClientResponseDto>>
    {
        /// <summary>
        /// Terme de recherche (nom partiel)
        /// </summary>
        public string SearchTerm { get; init; } = string.Empty;
        
        /// <summary>
        /// Nombre maximum de résultats
        /// </summary>
        public int Limit { get; init; } = 10;
    }
}