// Application/Features/SuivisJournaliers/Commands/UpdateSuivi/UpdateSuiviCommandHandler.cs
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.SuivisJournaliers.Dtos;
using Application.Features.Bandes.Interfaces;
using Application.Features.Stocks.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.SuivisJournaliers.Commands.UpdateSuivi
{
    /// <summary>
    /// Handler pour mettre à jour un suivi journalier.
    /// Gère les modifications et leurs impacts (stocks, bande).
    /// </summary>
    public class UpdateSuiviCommandHandler : IRequestHandler<UpdateSuiviCommand, SuiviResponseDto>
    {
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateSuiviCommandHandler> _logger;

        public UpdateSuiviCommandHandler(
            ISuiviJournalierRepository suiviRepository,
            IBandeRepository bandeRepository,
            IStockRepository stockRepository,
            IMapper mapper,
            ILogger<UpdateSuiviCommandHandler> logger)
        {
            _suiviRepository = suiviRepository;
            _bandeRepository = bandeRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SuiviResponseDto> Handle(
            UpdateSuiviCommand request, 
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateSuiviDto;

            _logger.LogInformation("Mise à jour du suivi {SuiviId}", updateDto.Id);

            // ============================================
            // 1. Récupérer le suivi existant
            // ============================================
            var existingSuivi = await _suiviRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingSuivi == null)
            {
                throw new NotFoundException($"Suivi avec l'ID {updateDto.Id} non trouvé.");
            }

            // ============================================
            // 2. Récupérer la bande associée
            // ============================================
            var bande = await _bandeRepository.GetByIdAsync(existingSuivi.BandeId, cancellationToken);
            if (bande == null)
            {
                throw new NotFoundException($"Bande avec l'ID {existingSuivi.BandeId} non trouvée.");
            }

            // ============================================
            // 3. Calculer les différences pour les impacts
            // ============================================
            var anciennesMortalites = existingSuivi.NbMortalites;
            var nouveauStockId = updateDto.StockConsommeId;
            var ancienStockId = existingSuivi.StockConsommeId;
            var nouvelleQuantite = updateDto.QuantiteConsommee ?? 0;
            var ancienneQuantite = existingSuivi.QuantiteConsommee ?? 0;

            // ============================================
            // 4. Gérer les changements de mortalité
            // ============================================
            if (updateDto.NbMortalites.HasValue && updateDto.NbMortalites != anciennesMortalites)
            {
                var differenceMortalites = updateDto.NbMortalites.Value - anciennesMortalites;
                
                // Mettre à jour la bande
                bande.NbPoussinsActuel -= differenceMortalites;
                bande.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInformation(
                    "Mise à jour des mortalités pour la bande {BandeId}: {Difference} (ancien: {Ancien}, nouveau: {Nouveau})",
                    bande.Id, differenceMortalites, anciennesMortalites, updateDto.NbMortalites);
            }

            // ============================================
            // 5. Gérer les changements de consommation de stock
            // ============================================
            if (nouveauStockId.HasValue)
            {
                await HandleStockChangesAsync(
                    existingSuivi,
                    updateDto,
                    ancienStockId,
                    nouveauStockId.Value,
                    ancienneQuantite,
                    nouvelleQuantite,
                    cancellationToken);
            }

            // ============================================
            // 6. Mettre à jour les propriétés
            // ============================================
            UpdateSuiviProperties(existingSuivi, updateDto);

            // ============================================
            // 7. Sauvegarder les modifications
            // ============================================
            await _suiviRepository.UpdateAsync(existingSuivi, cancellationToken);
            
            // Sauvegarder les modifications de la bande si nécessaire
            if (bande.UpdatedAt != DateTime.MinValue)
            {
                await _bandeRepository.UpdateAsync(bande, cancellationToken);
            }

            _logger.LogInformation("Suivi {SuiviId} mis à jour avec succès", updateDto.Id);

            // ============================================
            // 8. Retourner la réponse
            // ============================================
            return _mapper.Map<SuiviResponseDto>(existingSuivi);
        }

        /// <summary>
        /// Met à jour les propriétés du suivi
        /// </summary>
        private void UpdateSuiviProperties(SuiviJournalier suivi, UpdateSuiviDto dto)
        {
            if (dto.NbMortalites.HasValue)
                suivi.NbMortalites = dto.NbMortalites.Value;
            
            suivi.CauseMortalite = dto.CauseMortalite ?? suivi.CauseMortalite;
            
            if (dto.PoidsMoyen.HasValue)
                suivi.PoidsMoyen = dto.PoidsMoyen;
            
            if (dto.NbSujetsPeses.HasValue)
                suivi.NbSujetsPeses = dto.NbSujetsPeses;
            
            if (!string.IsNullOrEmpty(dto.TypeSoin))
                suivi.TypeSoin = Enum.Parse<TypeSoin>(dto.TypeSoin, true);
            
            suivi.ProduitUtilise = dto.ProduitUtilise ?? suivi.ProduitUtilise;
            suivi.Dosage = dto.Dosage ?? suivi.Dosage;
            
            if (dto.DureeTraitement.HasValue)
                suivi.DureeTraitement = dto.DureeTraitement;
            
            if (dto.CoutSoin.HasValue)
                suivi.CoutSoin = dto.CoutSoin;
            
            if (dto.StockConsommeId.HasValue)
                suivi.StockConsommeId = dto.StockConsommeId;
            
            if (dto.QuantiteConsommee.HasValue)
                suivi.QuantiteConsommee = dto.QuantiteConsommee;
            
            if (dto.Temperature.HasValue)
                suivi.Temperature = dto.Temperature;
            
            suivi.Observations = dto.Observations ?? suivi.Observations;
            
            suivi.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gère les changements de consommation de stock
        /// </summary>
        private async Task HandleStockChangesAsync(
            SuiviJournalier suivi,
            UpdateSuiviDto dto,
            int? ancienStockId,
            int nouveauStockId,
            decimal ancienneQuantite,
            decimal nouvelleQuantite,
            CancellationToken cancellationToken)
        {
            // Récupérer le nouveau stock
            var nouveauStock = await _stockRepository.GetByIdAsync(nouveauStockId, cancellationToken);
            if (nouveauStock == null)
            {
                throw new NotFoundException($"Stock avec l'ID {nouveauStockId} non trouvé.");
            }

            // Si le stock a changé
            if (ancienStockId != nouveauStockId)
            {
                // Restaurer l'ancien stock si existant
                if (ancienStockId.HasValue)
                {
                    var ancienStock = await _stockRepository.GetByIdAsync(ancienStockId.Value, cancellationToken);
                    if (ancienStock != null)
                    {
                        ancienStock.QuantiteActuelle += ancienneQuantite;
                        await _stockRepository.UpdateAsync(ancienStock, cancellationToken);
                    }
                }

                // Vérifier le nouveau stock
                if (nouveauStock.QuantiteActuelle < nouvelleQuantite)
                {
                    throw new BusinessRuleException(
                        $"Stock insuffisant. Disponible: {nouveauStock.QuantiteActuelle} {nouveauStock.UniteMesure}");
                }

                // Consommer le nouveau stock
                nouveauStock.QuantiteActuelle -= nouvelleQuantite;
                await _stockRepository.UpdateAsync(nouveauStock, cancellationToken);
            }
            else // Même stock, quantité différente
            {
                var differenceQuantite = nouvelleQuantite - ancienneQuantite;
                
                if (differenceQuantite > 0) // On consomme plus
                {
                    if (nouveauStock.QuantiteActuelle < differenceQuantite)
                    {
                        throw new BusinessRuleException(
                            $"Stock insuffisant pour la consommation supplémentaire. " +
                            $"Disponible: {nouveauStock.QuantiteActuelle} {nouveauStock.UniteMesure}");
                    }
                    nouveauStock.QuantiteActuelle -= differenceQuantite;
                }
                else if (differenceQuantite < 0) // On consomme moins
                {
                    nouveauStock.QuantiteActuelle += Math.Abs(differenceQuantite);
                }

                await _stockRepository.UpdateAsync(nouveauStock, cancellationToken);
            }
        }
    }
}