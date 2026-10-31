using MediatR;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Queries.GetAllAlertes
{
    public record GetAllAlertesQuery : IRequest<List<AlerteResponseDto>>
    {
        /// <summary>
        /// Filtrer par type d'alerte (optionnel)
        /// </summary>
        public string? TypeAlerte { get; init; }
        
        /// <summary>
        /// Filtrer par niveau (optionnel)
        /// </summary>
        public string? Niveau { get; init; }
        
        /// <summary>
        /// Uniquement les alertes non lues
        /// </summary>
        public bool? OnlyNonLues { get; init; }
        
        /// <summary>
        /// Uniquement les alertes d'une entité spécifique
        /// </summary>
        public int? EntiteId { get; init; }
        
        /// <summary>
        /// Date de début (optionnel)
        /// </summary>
        public DateTime? DateDebut { get; init; }
        
        /// <summary>
        /// Date de fin (optionnel)
        /// </summary>
        public DateTime? DateFin { get; init; }
    }
}