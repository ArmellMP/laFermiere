using MediatR;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Commands.CreateClient
{
    /// <summary>
    /// Commande pour créer un nouveau client.
    /// </summary>
    public record CreateClientCommand : IRequest<ClientResponseDto>
    {
        /// <summary>
        /// Données de création du client.
        /// </summary>
        public CreateClientDto CreateClientDto { get; init; } = null!;
    }
}