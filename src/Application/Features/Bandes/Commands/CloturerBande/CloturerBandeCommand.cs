using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Commands.CloturerBande
{
    /// <summary>
    /// Command pour clôturer une bande.
    /// Opération métier importante qui change le statut de la bande.
    /// </summary>
    public record CloturerBandeCommand : IRequest<BandeResponseDto>
    {
        /// <summary>
        /// ID de la bande à clôturer.
        /// </summary>
        public int Id { get; init; }
        
        /// <summary>
        /// Date de clôture (optionnelle, par défaut = maintenant).
        /// </summary>
        public DateTime? DateCloture { get; init; }
        
        /// <summary>
        /// Observations de clôture.
        /// </summary>
        public string? ObservationsCloture { get; init; }
    }
}