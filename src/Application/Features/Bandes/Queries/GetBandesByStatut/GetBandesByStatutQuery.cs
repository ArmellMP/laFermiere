using Application.Features.Bandes.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Features.Bandes.Queries.GetBandesByStatut
{
    public record GetBandesByStatutQuery : IRequest<List<BandeResponseDto>>
    {
        /// <summary>
        /// Statut des bandes à filtrer.
        /// </summary>
        public StatutBande Statut { get; init; }
        
        /// <summary>
        /// Inclure les détails supplémentaires. Par défaut : false.
        /// </summary>
        public bool IncludeDetails { get; init; } = false;
    }
}