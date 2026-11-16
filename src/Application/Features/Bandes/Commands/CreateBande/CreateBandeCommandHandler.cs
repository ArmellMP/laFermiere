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
        private async Task<string> GenerateCodeIfNeededAsync(CreateBandeDto dto, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(dto.Code))
                return dto.Code;

            return await GenerateBandeCodeAsync(dto.DateArrivee, cancellationToken);
        }

        /// <summary>
        /// Génère un code unique pour une bande.
        /// Format : ANNEE-MOIS-JOUR-NUM (ex: 2024-01-08-001)
        ///
        /// NUM = premier numéro libre pour cette date : 001, sinon 002, etc.
        /// (Avant : toujours "001". Deux bandes arrivées le même jour avaient le même code,
        ///  et la contrainte UNIQUE de la colonne Code provoquait une erreur 500.
        ///  Bug trouvé par le test d'intégration DeuxBandesSansCodeLeMemeJour_CodesDifferents.)
        /// </summary>
        private async Task<string> GenerateBandeCodeAsync(DateTime dateArrivee, CancellationToken cancellationToken)
        {
            var datePart = dateArrivee.ToString("yyyy-MM-dd");

            // 999 bandes le même jour : largement au-delà d'un élevage réel
            for (var numero = 1; numero <= 999; numero++)
            {
                var code = $"{datePart}-{numero:000}";   // :000 = sur 3 chiffres (1 -> "001")
                if (!await _bandeRepository.CodeExistsAsync(code, cancellationToken))
                {
                    return code;
                }
            }

            throw new BusinessRuleException(
                $"Plus aucun code automatique disponible pour le {dateArrivee:dd/MM/yyyy}. Indiquez un code manuellement.");
        }
    }
}