using MediatR;

namespace Application.Features.Clients.Commands.DeleteClient
{
    /// <summary>
    /// Commande pour supprimer un client.
    /// </summary>
    public record DeleteClientCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}