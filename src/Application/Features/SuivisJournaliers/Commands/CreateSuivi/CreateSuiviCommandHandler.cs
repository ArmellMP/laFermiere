using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.Stocks.Interfaces;
using Application.Features.SuivisJournaliers.Dtos;
using Application.Features.Bandes.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.SuivisJournaliers.Commands.CreateSuivi
{
    /// <summary>
    /// Handler pour créer un nouveau suivi journalier.
    /// </summary>
    public class CreateSuiviCommandHandler : IRequestHandler<CreateSuiviCommand, SuiviResponseDto>
    {
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSuiviCommandHandler> _logger;

        public CreateSuiviCommandHandler(
            ISuiviJournalierRepository suiviRepository,
            IBandeRepository bandeRepository,
            IStockRepository stockRepository,
            IMapper mapper,
            ILogger<CreateSuiviCommandHandler> logger)
        {
            _suiviRepository = suiviRepository;
            _bandeRepository = bandeRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
            _logger = logger;
        }      

        public async Task<SuiviResponseDto> Handle(
            CreateSuiviCommand request, 
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateSuiviDto;

            _logger.LogInformation("Création d'un suivi journalier pour la bande {BandeId}, jour {Jour}", 
                createDto.BandeId, createDto.Jour);

            // ============================================
            // 1. Vérification de la bande
            // ============================================
            var bande = await _bandeRepository.GetByIdAsync(createDto.BandeId, cancellationToken);
            if (bande == null)
            {
                throw new NotFoundException($"Bande avec l'ID {createDto.BandeId} non trouvée.");
            }

            // ============================================
            // 2. Vérification de l'unicité (1 suivi par jour par bande)
            // ============================================
            var exists = await _suiviRepository.ExistsForDateAsync(
                createDto.BandeId, 
                createDto.DateSuivi, 
                cancellationToken);
                
            if (exists)
            {
                throw new BusinessRuleException(
                    $"Un suivi existe déjà pour la bande {createDto.BandeId} à la date {createDto.DateSuivi:yyyy-MM-dd}.");
            }

            // ============================================
            // 3. Vérification de la cohérence du jour
            // ============================================
            var agePrevu = (createDto.DateSuivi - bande.DateArrivee).Days + 1;
            if (createDto.Jour != agePrevu)
            {
                _logger.LogWarning(
                    "Incohérence de jour: attendu {AgePrevu}, reçu {JourReçu}", 
                    agePrevu, createDto.Jour);
                // On peut soit lever une exception, soit corriger automatiquement
                createDto.Jour = agePrevu;
            }

            // ============================================
            // 4. Vérification du stock si consommation
            // ============================================
            if (createDto.StockConsommeId.HasValue && createDto.QuantiteConsommee.HasValue)
            {
                await ValidateStockConsumptionAsync(
                    createDto.StockConsommeId.Value,
                    createDto.QuantiteConsommee.Value,
                    cancellationToken);
            }

            // ============================================
            // 5. Création de l'entité
            // ============================================
            var suivi = new SuiviJournalier
            {
                BandeId = createDto.BandeId,
                Jour = createDto.Jour,
                DateSuivi = createDto.DateSuivi,
                NbMortalites = createDto.NbMortalites ?? 0,
                CauseMortalite = createDto.CauseMortalite,
                PoidsMoyen = createDto.PoidsMoyen,
                NbSujetsPeses = createDto.NbSujetsPeses,
                TypeSoin = string.IsNullOrEmpty(createDto.TypeSoin) 
                    ? null 
                    : Enum.Parse<TypeSoin>(createDto.TypeSoin, true),
                ProduitUtilise = createDto.ProduitUtilise,
                Dosage = createDto.Dosage,
                DureeTraitement = createDto.DureeTraitement,
                CoutSoin = createDto.CoutSoin,
                StockConsommeId = createDto.StockConsommeId,
                QuantiteConsommee = createDto.QuantiteConsommee,
                Temperature = createDto.Temperature,
                Observations = createDto.Observations,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1 // À remplacer par l'utilisateur connecté plus tard
            };

            // ============================================
            // 6. Sauvegarde
            // ============================================
            var createdSuivi = await _suiviRepository.AddAsync(suivi, cancellationToken);

            _logger.LogInformation("Suivi créé avec succès: ID {SuiviId}", createdSuivi.Id);

            // ============================================
            // 7. Mise à jour de la bande si mortalités
            // ============================================
            if (createDto.NbMortalites.HasValue && createDto.NbMortalites.Value > 0)
            {
                bande.NbPoussinsActuel -= createDto.NbMortalites.Value;
                bande.UpdatedAt = DateTime.UtcNow;
                await _bandeRepository.UpdateAsync(bande, cancellationToken);
                
                _logger.LogInformation(
                    "Mise à jour de la bande {BandeId}: {Mortalites} mortalités enregistrées, nouveau total {NbRestants}",
                    bande.Id, createDto.NbMortalites, bande.NbPoussinsActuel);
            }

            // ============================================
            // 8. Retour de la réponse
            // ============================================
            return _mapper.Map<SuiviResponseDto>(createdSuivi);
        }

        /// <summary>
        /// Valide que la consommation de stock est possible
        /// </summary>
        private async Task ValidateStockConsumptionAsync(
            int stockId, 
            decimal quantite, 
            CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByIdAsync(stockId, cancellationToken);
            if (stock == null)
            {
                throw new NotFoundException($"Stock avec l'ID {stockId} non trouvé.");
            }

            if (stock.QuantiteActuelle < quantite)
            {
                throw new BusinessRuleException(
                    $"Stock insuffisant. Disponible: {stock.QuantiteActuelle} {stock.UniteMesure}, Demandé: {quantite}");
            }

            if (stock.DatePeremption < DateTime.Now)
            {
                throw new BusinessRuleException(
                    $"Impossible d'utiliser un stock périmé (périmé depuis le {stock.DatePeremption:yyyy-MM-dd}).");
            }
        }
    }
}