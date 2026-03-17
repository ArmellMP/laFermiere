using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Commands.CreateBande
{
    /// <summary>
    /// Commande pour créer une nouvelle bande.
    /// </summary>
    public record CreateBandeCommand : IRequest<BandeResponseDto>
    {
        /// <summary>
        /// Données de création de la bande.
        /// </summary>
        public CreateBandeDto CreateBandeDto { get; init; } = null!;
    }
}