using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Alertes.Interfaces;
using Application.Features.Alertes.Dtos;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Alertes.Commands.CreateAlerte
{
    public class CreateAlerteCommandHandler : IRequestHandler<CreateAlerteCommand, AlerteResponseDto>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<CreateAlerteCommandHandler> _logger;

        public CreateAlerteCommandHandler(
            IAlerteRepository alerteRepository,
            ILogger<CreateAlerteCommandHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _logger = logger;
        }

        public async Task<AlerteResponseDto> Handle(
            CreateAlerteCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateAlerteDto;

            _logger.LogInformation("Création d'une nouvelle alerte : {TypeAlerte}", createDto.TypeAlerte);

            // ============================================
            // 1. Conversion des enums
            // ============================================
            if (!Enum.TryParse<TypeAlerte>(createDto.TypeAlerte, true, out var typeAlerte))
            {
                throw new BusinessRuleException($"Type d'alerte invalide : {createDto.TypeAlerte}");
            }

            if (!Enum.TryParse<NiveauAlerte>(createDto.Niveau, true, out var niveau))
            {
                throw new BusinessRuleException($"Niveau d'alerte invalide : {createDto.Niveau}");
            }

            // ============================================
            // 2. Création de l'entité
            // ============================================
            var alerte = new Alerte
            {
                TypeAlerte = typeAlerte,
                EntiteId = createDto.EntiteId,
                Niveau = niveau,
                Message = createDto.Message,
                EstLue = false,
                DateAlerte = DateTime.UtcNow
            };

            // ============================================
            // 3. Sauvegarde
            // ============================================
            var createdAlerte = await _alerteRepository.AddAsync(alerte, cancellationToken);

            _logger.LogInformation("Alerte créée avec succès : ID {AlerteId}", createdAlerte.Id);

            // ============================================
            // 4. Retour de la réponse
            // ============================================
            return createdAlerte.ToResponseDto();
        }
    }
}