using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Alertes.Interfaces;
using Application.Features.Alertes.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Alertes.Commands.UpdateAlerte
{
    public class UpdateAlerteCommandHandler : IRequestHandler<UpdateAlerteCommand, AlerteResponseDto>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<UpdateAlerteCommandHandler> _logger;

        public UpdateAlerteCommandHandler(
            IAlerteRepository alerteRepository,
            ILogger<UpdateAlerteCommandHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _logger = logger;
        }

        public async Task<AlerteResponseDto> Handle(
            UpdateAlerteCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateAlerteDto;

            _logger.LogInformation("Mise à jour de l'alerte {AlerteId}", updateDto.Id);

            // 1. Récupérer l'alerte existante
            var existingAlerte = await _alerteRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingAlerte == null)
            {
                throw new NotFoundException($"Alerte avec l'ID {updateDto.Id} non trouvée.");
            }

            // 2. Si on marque comme lue
            if (updateDto.EstLue.HasValue && updateDto.EstLue.Value && !existingAlerte.EstLue)
            {
                if (!updateDto.UtilisateurId.HasValue)
                {
                    throw new BusinessRuleException("L'ID de l'utilisateur est requis pour marquer une alerte comme lue.");
                }
                existingAlerte.MarquerCommeLue(updateDto.UtilisateurId.Value);
                _logger.LogInformation("Alerte {AlerteId} marquée comme lue par l'utilisateur {UtilisateurId}", 
                    updateDto.Id, updateDto.UtilisateurId);
            }

            // 3. Sauvegarde
            await _alerteRepository.UpdateAsync(existingAlerte, cancellationToken);

            _logger.LogInformation("Alerte {AlerteId} mise à jour avec succès", updateDto.Id);

            // 4. Retour de la réponse
            return existingAlerte.ToResponseDto();
        }
    }
}