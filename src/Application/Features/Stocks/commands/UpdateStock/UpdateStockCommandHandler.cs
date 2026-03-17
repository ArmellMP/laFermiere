using MediatR;
using AutoMapper;
using Application.Common.Exceptions;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Commands.UpdateStock
{
    /// <summary>
    /// Handler pour mettre à jour un stock existant.
    /// </summary>
    public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, StockResponseDto>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public UpdateStockCommandHandler(
            IStockRepository stockRepository,
            IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<StockResponseDto> Handle(
            UpdateStockCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer le DTO de mise à jour
            var updateDto = request.UpdateStockDto;
            
            // 2. Récupérer le stock existant
            var existingStock = await GetExistingStockAsync(updateDto.Id, cancellationToken);
            
            // 3. Valider les règles métier de mise à jour
            await ValidateUpdateRulesAsync(existingStock, updateDto, cancellationToken);
            
            // 4. Mettre à jour les propriétés modifiables
            UpdateStockProperties(existingStock, updateDto);
            
            // 5. Mettre à jour le timestamp
            existingStock.UpdatedAt = DateTime.Now;
            
            // 6. Sauvegarder les modifications
            await _stockRepository.UpdateAsync(existingStock, cancellationToken);
            
            // 7. Retourner le DTO de réponse
            return _mapper.Map<StockResponseDto>(existingStock);
        }
        
        /// <summary>
        /// Récupère le stock existant ou lance une exception.
        /// </summary>
        private async Task<Domain.Entities.Stock> GetExistingStockAsync(
            int stockId, 
            CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByIdAsync(stockId, cancellationToken);
            
            if (stock == null)
            {
                throw new NotFoundException($"Stock avec l'ID {stockId} non trouvé.");
            }
            
            return stock;
        }
        
        /// <summary>
        /// Valide les règles métier spécifiques à la mise à jour.
        /// </summary>
        private async Task ValidateUpdateRulesAsync(
            Domain.Entities.Stock existingStock,
            UpdateStockDto updateDto,
            CancellationToken cancellationToken)
        {
            // Règle 1 : Ne pas pouvoir diminuer la quantité en dessous de 0 (déjà validé par le DTO)
            
            // Règle 2 : Vérifier que le stock n'est pas périmé si on augmente la quantité
            if (updateDto.QuantiteActuelle > existingStock.QuantiteActuelle && 
                existingStock.DatePeremption < DateTime.Now)
            {
                throw new BusinessRuleException(
                    "Impossible d'augmenter la quantité d'un stock périmé.");
            }
            
            // Règle 3 : Vérifier que le nouveau seuil d'alerte est cohérent
            if (updateDto.SeuilAlerte.HasValue && 
                updateDto.SeuilAlerte > existingStock.QuantiteInitiale)
            {
                throw new BusinessRuleException(
                    "Le seuil d'alerte ne peut pas dépasser la quantité initiale.");
            }
            
            // Règle 4 : Empêcher certaines modifications si le stock a été consommé
            var hasConsommation = await _stockRepository.HasConsommationAsync(
                existingStock.Id, cancellationToken);
                
            if (hasConsommation)
            {
                // Si le stock a déjà été utilisé, on ne peut plus changer certaines propriétés
                // Par exemple, on ne peut pas changer le type, l'unité de mesure, etc.
                // Mais ces propriétés ne sont pas dans UpdateStockDto, donc pas de problème
            }
        }
        
        /// <summary>
        /// Met à jour les propriétés modifiables du stock.
        /// </summary>
        private void UpdateStockProperties(
            Domain.Entities.Stock stock, 
            UpdateStockDto dto)
        {
            // Seules ces propriétés sont modifiables
            stock.QuantiteActuelle = dto.QuantiteActuelle;
            stock.SeuilAlerte = dto.SeuilAlerte ?? stock.SeuilAlerte;
            stock.DatePeremption = dto.DatePeremption ?? stock.DatePeremption;
            stock.PrixUnitaire = dto.PrixUnitaire ?? stock.PrixUnitaire;
            
            // Note: Le Type, le Nom, l'Unité de mesure, la QuantiteInitiale
            // ne sont PAS modifiables après création
        }
    }
}