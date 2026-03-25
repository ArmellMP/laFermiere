using MediatR;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Commands.UpdateClient
{
    /// <summary>
    /// Commande pour mettre à jour un client existant.
    /// </summary>
    public record UpdateClientCommand : IRequest<ClientResponseDto>
    {
        /// <summary>
        /// Données de mise à jour du client.
        /// </summary>
        public UpdateClientDto UpdateClientDto { get; init; } = null!;
    }
}