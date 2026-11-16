using Application.Common.Exceptions;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Application.Common.Mappings;

namespace Application.Features.Bandes.Commands.CreateBande
{
    /// <summary>
    /// Handler pour créer une nouvelle bande.
    /// </summary>
    public class CreateBandeCommandHandler 
        : IRequestHandler<CreateBandeCommand, BandeResponseDto>
    {
        private readonly IBandeRepository _bandeRepository;

        public CreateBandeCommandHandler(
            IBandeRepository bandeRepository)
        {
            _bandeRepository = bandeRepository;
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
                Code = await GenerateCodeIfNeededAsync(createDto, cancellationToken),
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
            var bandeDto = createdBande.ToResponseDto();
            
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
        private async Task<string> GenerateCodeIfNeededAsync(
            CreateBandeDto dto,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(dto.Code))
                return dto.Code;

            return await GenerateBandeCodeAsync(dto.DateArrivee, cancellationToken);
        }

        /// <summary>
        /// Génère un code unique pour une bande.
        /// Format : ANNEE-MOIS-JOUR-NUM (ex: 2024-01-08-001, puis 2024-01-08-002
        /// pour une deuxième bande arrivée le même jour).
        /// La contrainte UNIQUE sur Bandes.Code reste le garde-fou final si deux
        /// créations simultanées obtenaient le même numéro.
        /// </summary>
        private async Task<string> GenerateBandeCodeAsync(
            DateTime dateArrivee,
            CancellationToken cancellationToken)
        {
            var numero = await _bandeRepository.GetProchainNumeroCodeAsync(dateArrivee, cancellationToken);

            return $"{dateArrivee:yyyy-MM-dd}-{numero:D3}";
        }
    }
}