using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Clients.Interfaces;

namespace Application.Features.Clients.Commands.DeleteClient
{
    /// <summary>
    /// Handler pour supprimer un client.
    /// </summary>
    public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Unit>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<DeleteClientCommandHandler> _logger;

        public DeleteClientCommandHandler(
            IClientRepository clientRepository,
            ILogger<DeleteClientCommandHandler> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteClientCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression du client {ClientId}", request.Id);

            // ============================================
            // 1. Vérifier que le client existe
            // ============================================
            var exists = await _clientRepository.ExistsAsync(request.Id, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException($"Client avec l'ID {request.Id} non trouvé.");
            }

            // ============================================
            // 2. Vérifier que le client n'a pas de ventes
            // ============================================
            var hasVentes = await _clientRepository.HasVentesAsync(request.Id, cancellationToken);
            if (hasVentes)
            {
                throw new BusinessRuleException(
                    "Impossible de supprimer un client qui a déjà des ventes. " +
                    "Vous pouvez le désactiver à la place.");
            }

            // ============================================
            // 3. Supprimer le client
            // ============================================
            await _clientRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Client {ClientId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}