using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Alertes.Interfaces;

namespace Application.Features.Alertes.Commands.MarquerAlertes
{
    public class MarquerAlertesCommandHandler : IRequestHandler<MarquerAlertesCommand, Unit>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<MarquerAlertesCommandHandler> _logger;

        public MarquerAlertesCommandHandler(
            IAlerteRepository alerteRepository,
            ILogger<MarquerAlertesCommandHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            MarquerAlertesCommand request,
            CancellationToken cancellationToken)
        {
            if (request.MarquerToutes)
            {
                _logger.LogInformation("Marquage de toutes les alertes comme lues par l'utilisateur {UtilisateurId}", 
                    request.UtilisateurId);
                await _alerteRepository.MarquerToutesCommeLuesAsync(request.UtilisateurId, cancellationToken);
            }
            else if (request.AlerteIds.Any())
            {
                _logger.LogInformation("Marquage de {Count} alertes comme lues par l'utilisateur {UtilisateurId}", 
                    request.AlerteIds.Count(), request.UtilisateurId);
                await _alerteRepository.MarquerPlusieursCommeLuesAsync(request.AlerteIds, request.UtilisateurId, cancellationToken);
            }

            return Unit.Value;
        }
    }
}