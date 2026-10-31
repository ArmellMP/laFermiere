using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Parametres.Interfaces;
using Application.Features.Parametres.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Parametres.Commands.CreateParametre
{
    public class CreateParametreCommandHandler : IRequestHandler<CreateParametreCommand, ParametreResponseDto>
    {
        private readonly IParametreRepository _parametreRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateParametreCommandHandler> _logger;

        public CreateParametreCommandHandler(
            IParametreRepository parametreRepository,
            IMapper mapper,
            ILogger<CreateParametreCommandHandler> logger)
        {
            _parametreRepository = parametreRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ParametreResponseDto> Handle(
            CreateParametreCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateParametreDto;

            _logger.LogInformation("Création d'un nouveau paramètre : {Cle}", createDto.Cle);

            // ============================================
            // 1. Vérifier que la clé n'existe pas déjà
            // ============================================
            var cleExists = await _parametreRepository.CleExistsAsync(createDto.Cle, null, cancellationToken);
            if (cleExists)
            {
                throw new BusinessRuleException($"Un paramètre avec la clé '{createDto.Cle}' existe déjà.");
            }

            // ============================================
            // 2. Conversion de la catégorie (string → enum)
            // ============================================
            if (!Enum.TryParse<CategorieParametre>(createDto.Categorie, true, out var categorie))
            {
                throw new BusinessRuleException($"Catégorie invalide : {createDto.Categorie}");
            }

            // ============================================
            // 3. Création de l'entité
            // ============================================
            var parametre = new Parametre
            {
                Cle = createDto.Cle,
                Valeur = createDto.Valeur,
                Description = createDto.Description,
                Categorie = categorie,
                EstModifiable = createDto.EstModifiable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // ============================================
            // 4. Sauvegarde
            // ============================================
            var createdParametre = await _parametreRepository.AddAsync(parametre, cancellationToken);

            _logger.LogInformation("Paramètre créé avec succès : ID {ParametreId}, Clé {Cle}",
                createdParametre.Id, createdParametre.Cle);

            // ============================================
            // 5. Retour de la réponse
            // ============================================
            return _mapper.Map<ParametreResponseDto>(createdParametre);
        }
    }
}