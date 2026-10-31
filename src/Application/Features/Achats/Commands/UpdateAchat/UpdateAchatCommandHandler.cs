using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Achats.Commands.UpdateAchat
{
    public class UpdateAchatCommandHandler : IRequestHandler<UpdateAchatCommand, AchatResponseDto>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<UpdateAchatCommandHandler> _logger;

        public UpdateAchatCommandHandler(
            IAchatRepository achatRepository,
            ILogger<UpdateAchatCommandHandler> logger)
        {
            _achatRepository = achatRepository;
            _logger = logger;
        }

        public async Task<AchatResponseDto> Handle(
            UpdateAchatCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateAchatDto;

            _logger.LogInformation("Mise à jour de l'achat {AchatId}", updateDto.Id);

            // 1. Récupérer l'achat existant
            var existingAchat = await _achatRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingAchat == null)
            {
                throw new NotFoundException($"Achat avec l'ID {updateDto.Id} non trouvé.");
            }

            // 2. Mise à jour des propriétés
            if (updateDto.DateAchat.HasValue)
                existingAchat.DateAchat = updateDto.DateAchat.Value;

            if (!string.IsNullOrEmpty(updateDto.TypeAchat))
            {
                if (Enum.TryParse<TypeAchat>(updateDto.TypeAchat, true, out var typeAchat))
                {
                    existingAchat.TypeAchat = typeAchat;
                }
            }

            existingAchat.Description = updateDto.Description ?? existingAchat.Description;

            if (updateDto.Quantite.HasValue)
                existingAchat.Quantite = updateDto.Quantite.Value;

            if (!string.IsNullOrEmpty(updateDto.UniteMesure))
            {
                if (Enum.TryParse<UniteMesureAchat>(updateDto.UniteMesure, true, out var uniteMesure))
                {
                    existingAchat.UniteMesure = uniteMesure;
                }
            }

            if (updateDto.PrixUnitaire.HasValue)
            {
                existingAchat.PrixUnitaire = updateDto.PrixUnitaire.Value;
                // Recalculer le montant total
                existingAchat.MontantTotal = existingAchat.Quantite * existingAchat.PrixUnitaire;
            }

            existingAchat.FactureNumero = updateDto.FactureNumero ?? existingAchat.FactureNumero;
            existingAchat.DateReception = updateDto.DateReception ?? existingAchat.DateReception;
            existingAchat.Observations = updateDto.Observations ?? existingAchat.Observations;

            // 3. Sauvegarde
            await _achatRepository.UpdateAsync(existingAchat, cancellationToken);

            _logger.LogInformation("Achat {AchatId} mis à jour avec succès", updateDto.Id);

            // 4. Retour de la réponse
            return existingAchat.ToResponseDto();
        }
    }
}