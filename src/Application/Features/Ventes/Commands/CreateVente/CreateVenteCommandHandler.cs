using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;
using Application.Features.Bandes.Interfaces;
using Application.Features.Clients.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Ventes.Commands.CreateVente
{
    public class CreateVenteCommandHandler : IRequestHandler<CreateVenteCommand, VenteResponseDto>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<CreateVenteCommandHandler> _logger;

        public CreateVenteCommandHandler(
            IVenteRepository venteRepository,
            IBandeRepository bandeRepository,
            IClientRepository clientRepository,
            ILogger<CreateVenteCommandHandler> logger)
        {
            _venteRepository = venteRepository;
            _bandeRepository = bandeRepository;
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<VenteResponseDto> Handle(
            CreateVenteCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateVenteDto;

            _logger.LogInformation("Création d'une vente pour la bande {BandeId}, client {ClientId}", 
                createDto.BandeId, createDto.ClientId);

            // ============================================
            // 1. Vérifier que la bande existe
            // ============================================
            var bande = await _bandeRepository.GetByIdAsync(createDto.BandeId, cancellationToken);
            if (bande == null)
            {
                throw new NotFoundException($"Bande avec l'ID {createDto.BandeId} non trouvée.");
            }

            // ============================================
            // 2. Vérifier que le client existe
            // ============================================
            var client = await _clientRepository.GetByIdAsync(createDto.ClientId, cancellationToken);
            if (client == null)
            {
                throw new NotFoundException($"Client avec l'ID {createDto.ClientId} non trouvé.");
            }

            // ============================================
            // 3. Vérifier que la bande a assez de poussins
            // ============================================
            if (createDto.TypeVente == "PouletsVivants" || createDto.TypeVente == "PouletsNettoyes" || createDto.TypeVente == "PouletsFumes")
            {
                if (bande.NbPoussinsActuel < (int)createDto.Quantite)
                {
                    throw new BusinessRuleException(
                        $"Stock insuffisant. Poussins disponibles: {bande.NbPoussinsActuel}, " +
                        $"Quantité demandée: {createDto.Quantite}");
                }
            }

            // ============================================
            // 4. Conversion des enums
            // ============================================
            if (!Enum.TryParse<TypeVente>(createDto.TypeVente, true, out var typeVente))
            {
                throw new BusinessRuleException($"Type de vente invalide : {createDto.TypeVente}");
            }

            if (!Enum.TryParse<UniteMesureVente>(createDto.UniteMesure, true, out var uniteMesure))
            {
                throw new BusinessRuleException($"Unité de mesure invalide : {createDto.UniteMesure}");
            }

            if (!Enum.TryParse<ModePaiement>(createDto.ModePaiement, true, out var modePaiement))
            {
                throw new BusinessRuleException($"Mode de paiement invalide : {createDto.ModePaiement}");
            }

            // ============================================
            // 5. Calcul du montant total
            // ============================================
            var montantTotal = createDto.Quantite * createDto.PrixUnitaire;

            // ============================================
            // 6. Création de l'entité
            // ============================================
            var vente = new Vente
            {
                BandeId = createDto.BandeId,
                ClientId = createDto.ClientId,
                DateVente = createDto.DateVente,
                TypeVente = typeVente,
                Quantite = createDto.Quantite,
                UniteMesure = uniteMesure,
                PrixUnitaire = createDto.PrixUnitaire,
                MontantTotal = montantTotal,
                ModePaiement = modePaiement,
                EstPaye = createDto.EstPaye,
                DatePaiement = createDto.DatePaiement,
                Observations = createDto.Observations,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1 // À remplacer par l'utilisateur connecté
            };

            // ============================================
            // 7. Sauvegarde
            // ============================================
            var createdVente = await _venteRepository.AddAsync(vente, cancellationToken);

            _logger.LogInformation("Vente créée avec succès : ID {VenteId}, Montant {MontantTotal}", 
                createdVente.Id, createdVente.MontantTotal);

            // ============================================
            // 8. Mettre à jour la bande (réduire le nombre de poussins)
            // ============================================
            if (createDto.TypeVente == "PouletsVivants" || createDto.TypeVente == "PouletsNettoyes" || createDto.TypeVente == "PouletsFumes")
            {
                bande.NbPoussinsActuel -= (int)createDto.Quantite;
                bande.UpdatedAt = DateTime.UtcNow;
                await _bandeRepository.UpdateAsync(bande, cancellationToken);
                
                _logger.LogInformation("Mise à jour de la bande {BandeId}: {Quantite} poussins vendus, nouveau total {NbRestants}",
                    bande.Id, createDto.Quantite, bande.NbPoussinsActuel);
            }

            // ============================================
            // 9. Retour de la réponse
            // ============================================
            return createdVente.ToResponseDto();
        }
    }
}