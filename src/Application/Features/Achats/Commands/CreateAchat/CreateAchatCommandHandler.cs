using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Application.Features.Bandes.Interfaces;
using Application.Features.Fournisseurs.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Achats.Commands.CreateAchat
{
    public class CreateAchatCommandHandler : IRequestHandler<CreateAchatCommand, AchatResponseDto>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateAchatCommandHandler> _logger;

        public CreateAchatCommandHandler(
            IAchatRepository achatRepository,
            IBandeRepository bandeRepository,
            IFournisseurRepository fournisseurRepository,
            IMapper mapper,
            ILogger<CreateAchatCommandHandler> logger)
        {
            _achatRepository = achatRepository;
            _bandeRepository = bandeRepository;
            _fournisseurRepository = fournisseurRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AchatResponseDto> Handle(
            CreateAchatCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateAchatDto;

            _logger.LogInformation("Création d'un achat pour la bande {BandeId}, fournisseur {FournisseurId}",
                createDto.BandeId, createDto.FournisseurId);

            // ============================================
            // 1. Vérifier que la bande existe
            // ============================================
            var bande = await _bandeRepository.GetByIdAsync(createDto.BandeId, cancellationToken);
            if (bande == null)
            {
                throw new NotFoundException($"Bande avec l'ID {createDto.BandeId} non trouvée.");
            }

            // ============================================
            // 2. Vérifier que le fournisseur existe
            // ============================================
            var fournisseur = await _fournisseurRepository.GetByIdAsync(createDto.FournisseurId, cancellationToken);
            if (fournisseur == null)
            {
                throw new NotFoundException($"Fournisseur avec l'ID {createDto.FournisseurId} non trouvé.");
            }

            // ============================================
            // 3. Conversion des enums
            // ============================================
            if (!Enum.TryParse<TypeAchat>(createDto.TypeAchat, true, out var typeAchat))
            {
                throw new BusinessRuleException($"Type d'achat invalide : {createDto.TypeAchat}");
            }

            if (!Enum.TryParse<UniteMesureAchat>(createDto.UniteMesure, true, out var uniteMesure))
            {
                throw new BusinessRuleException($"Unité de mesure invalide : {createDto.UniteMesure}");
            }

            // ============================================
            // 4. Calcul du montant total
            // ============================================
            var montantTotal = createDto.Quantite * createDto.PrixUnitaire;

            // ============================================
            // 5. Création de l'entité
            // ============================================
            var achat = new Achat
            {
                FournisseurId = createDto.FournisseurId,
                BandeId = createDto.BandeId,
                DateAchat = createDto.DateAchat,
                TypeAchat = typeAchat,
                Description = createDto.Description,
                Quantite = createDto.Quantite,
                UniteMesure = uniteMesure,
                PrixUnitaire = createDto.PrixUnitaire,
                MontantTotal = montantTotal,
                FactureNumero = createDto.FactureNumero,
                DateReception = createDto.DateReception,
                Observations = createDto.Observations,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1 // À remplacer par l'utilisateur connecté
            };

            // ============================================
            // 6. Sauvegarde
            // ============================================
            var createdAchat = await _achatRepository.AddAsync(achat, cancellationToken);

            _logger.LogInformation("Achat créé avec succès : ID {AchatId}, Montant {MontantTotal}",
                createdAchat.Id, createdAchat.MontantTotal);

            // ============================================
            // 7. Si c'est un achat de poussins, mettre à jour la bande
            // ============================================
            if (typeAchat == TypeAchat.Poussins)
            {
                bande.NbPoussinsInit += (int)createDto.Quantite;
                bande.NbPoussinsActuel += (int)createDto.Quantite;
                bande.UpdatedAt = DateTime.UtcNow;
                await _bandeRepository.UpdateAsync(bande, cancellationToken);
                
                _logger.LogInformation("Mise à jour de la bande {BandeId}: +{Quantite} poussins, nouveau total {NbTotal}",
                    bande.Id, createDto.Quantite, bande.NbPoussinsActuel);
            }

            // ============================================
            // 8. Retour de la réponse
            // ============================================
            return _mapper.Map<AchatResponseDto>(createdAchat);
        }
    }
}