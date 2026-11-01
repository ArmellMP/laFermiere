// Application/Features/Alertes/Commands/DeleteAlerte/DeleteAlerteCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Alertes.Interfaces;

namespace Application.Features.Alertes.Commands.DeleteAlerte
{
    public class DeleteAlerteCommandHandler : IRequestHandler<DeleteAlerteCommand, Unit>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<DeleteAlerteCommandHandler> _logger;

        public DeleteAlerteCommandHandler(
            IAlerteRepository alerteRepository,
            ILogger<DeleteAlerteCommandHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteAlerteCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression de l'alerte {AlerteId}", request.Id);

            // 1. Récupérer l'alerte pour vérifier son existence
            var alerte = await _alerteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (alerte == null)
            {
                throw new NotFoundException($"Alerte avec l'ID {request.Id} non trouvée.");
            }

            // 2. Supprimer l'alerte
            await _alerteRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Alerte {AlerteId} supprimée avec succès", request.Id);

            return Unit.Value;
        }
    }
}