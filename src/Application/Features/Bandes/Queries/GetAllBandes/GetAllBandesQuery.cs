using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Queries.GetAllBandes
{
    /// <summary>
    /// Query pour récupérer toutes les bandes.
    /// Utilise un record pour l'immutabilité.
    /// </summary>
    public record GetAllBandesQuery : IRequest<List<BandeResponseDto>>
    {
        /// <summary>
        /// Inclure les détails supplémentaires (mortalités, jours, etc.).
        /// Par défaut : false (pour la performance).
        /// </summary>
        public bool IncludeDetails { get; init; } = false;
    }
}