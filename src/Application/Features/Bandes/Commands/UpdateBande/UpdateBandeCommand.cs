using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Commands.UpdateBande
{
    /// <summary>
    /// Commande pour mettre à jour une bande existante.
    /// </summary>
    public record UpdateBandeCommand : IRequest<BandeResponseDto>
    {
        /// <summary>
        /// Données de mise à jour de la bande.
        /// </summary>
        public UpdateBandeDto UpdateBandeDto { get; init; } = null!;
    }
}