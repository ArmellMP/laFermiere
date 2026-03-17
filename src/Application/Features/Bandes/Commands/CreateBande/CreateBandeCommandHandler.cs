using Application.Common.Exceptions;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Bandes.Commands.CreateBande
{
    /// <summary>
    /// Handler pour créer une nouvelle bande.
    /// </summary>
    public class CreateBandeCommandHandler 
        : IRequestHandler<CreateBandeCommand, BandeResponseDto>
    {
        private readonly IBandeRepository _bandeRepository;
        private readonly IMapper _mapper;

        public CreateBandeCommandHandler(
            IBandeRepository bandeRepository,
            IMapper mapper)
        {
            _bandeRepository = bandeRepository;
            _mapper = mapper;
        }

        public async Task<BandeResponseDto> Handle(
            CreateBandeCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Vérifier l'unicité du code (si fourni)
            await ValidateUniqueCodeAsync(request, cancellationToken);
            
            var createDto = request.CreateBandeDto;
            
            // 2. Créer l'entité Domain (plus de AutoMapper ici pour plus de contrôle)
            var bande = new Bande
            {
                Code = GenerateCodeIfNeeded(createDto),
                DateArrivee = createDto.DateArrivee,
                NbPoussinsInit = createDto.NbPoussinsInit,
                NbPoussinsActuel = createDto.NbPoussinsInit, // Au départ = initial
                Observations = createDto.Observations,
                Statut = StatutBande.EnPreparation,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            // 3. Sauvegarder dans la base de données
            var createdBande = await _bandeRepository.AddAsync(bande, cancellationToken);
            
            // 4. Convertir en DTO pour la réponse
            var bandeDto = _mapper.Map<BandeResponseDto>(createdBande);
            
            return bandeDto;
        }
        
        /// <summary>
        /// Vérifie que le code est unique (s'il est fourni).
        /// </summary>
        private async Task ValidateUniqueCodeAsync(
            CreateBandeCommand request, 
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.CreateBandeDto.Code))
            {
                var codeExists = await _bandeRepository.CodeExistsAsync(
                    request.CreateBandeDto.Code, 
                    cancellationToken);
                    
                if (codeExists)
                {
                    throw new BusinessRuleException(
                        $"Le code '{request.CreateBandeDto.Code}' existe déjà. " +
                        "Veuillez en choisir un autre ou laisser le système en générer un.");
                }
            }
        }
        
        /// <summary>
        /// Génère un code si non fourni.
        /// </summary>
        private string GenerateCodeIfNeeded(CreateBandeDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Code))
                return dto.Code;
                
            return GenerateBandeCode(dto.DateArrivee);
        }
        
        /// <summary>
        /// Génère un code unique pour une bande.
        /// Format : ANNEE-MOIS-JOUR-NUM (ex: 2024-01-08-001)
        /// </summary>
        private string GenerateBandeCode(DateTime dateArrivee)
        {
            // TODO: Améliorer avec une vraie séquence depuis la base de données
            var datePart = dateArrivee.ToString("yyyy-MM-dd");
            var sequenceNumber = "001"; // À remplacer par une vraie logique
            
            return $"{datePart}-{sequenceNumber}";
        }
    }
}