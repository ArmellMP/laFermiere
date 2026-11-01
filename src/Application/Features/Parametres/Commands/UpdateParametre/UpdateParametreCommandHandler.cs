using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Parametres.Interfaces;
using Application.Features.Parametres.Dtos;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Parametres.Commands.UpdateParametre
{
    public class UpdateParametreCommandHandler : IRequestHandler<UpdateParametreCommand, ParametreResponseDto>
    {
        private readonly IParametreRepository _parametreRepository;
        private readonly ILogger<UpdateParametreCommandHandler> _logger;

        public UpdateParametreCommandHandler(
            IParametreRepository parametreRepository,
            ILogger<UpdateParametreCommandHandler> logger)
        {
            _parametreRepository = parametreRepository;
            _logger = logger;
        }

        public async Task<ParametreResponseDto> Handle(
            UpdateParametreCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateParametreDto;

            _logger.LogInformation("Mise à jour du paramètre {ParametreId}", updateDto.Id);

            // 1. Récupérer le paramètre existant
            var existingParametre = await _parametreRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingParametre == null)
            {
                throw new NotFoundException($"Paramètre avec l'ID {updateDto.Id} non trouvé.");
            }

            // 2. Vérifier si le paramètre est modifiable
            if (!existingParametre.EstModifiable && updateDto.Valeur != null)
            {
                throw new BusinessRuleException($"Le paramètre '{existingParametre.Cle}' n'est pas modifiable.");
            }

            // 3. Mise à jour des propriétés
            if (updateDto.Valeur != null)
                existingParametre.Valeur = updateDto.Valeur;

            if (updateDto.Description != null)
                existingParametre.Description = updateDto.Description;

            if (!string.IsNullOrEmpty(updateDto.Categorie))
            {
                if (Enum.TryParse<CategorieParametre>(updateDto.Categorie, true, out var categorie))
                {
                    existingParametre.Categorie = categorie;
                }
            }

            if (updateDto.EstModifiable.HasValue)
                existingParametre.EstModifiable = updateDto.EstModifiable.Value;

            existingParametre.UpdatedAt = DateTime.UtcNow;

            // 4. Sauvegarde
            await _parametreRepository.UpdateAsync(existingParametre, cancellationToken);

            _logger.LogInformation("Paramètre {ParametreId} mis à jour avec succès", updateDto.Id);

            // 5. Retour de la réponse
            return existingParametre.ToResponseDto();
        }
    }
}