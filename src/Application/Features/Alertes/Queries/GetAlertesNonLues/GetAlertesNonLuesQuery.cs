using MediatR;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Queries.GetAlertesNonLues
{
    public record GetAlertesNonLuesQuery : IRequest<List<AlerteResponseDto>>
    {
        /// <summary>
        /// Filtrer par type d'alerte (optionnel)
        /// </summary>
        public string? TypeAlerte { get; init; }
        
        /// <summary>
        /// Filtrer par niveau (optionnel)
        /// </summary>
        public string? Niveau { get; init; }
    }
}