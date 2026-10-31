using MediatR;
using Application.Common.Exceptions;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Stocks.Commands.ConsommerStock
{
    /// <summary>
    /// Handler pour consommer une quantité d'un stock.
    /// </summary>
    public class ConsommerStockCommandHandler : IRequestHandler<ConsommerStockCommand, StockResponseDto>
    {
        private readonly IStockRepository _stockRepository;

        public ConsommerStockCommandHandler(
            IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<StockResponseDto> Handle(
            ConsommerStockCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer le stock
            var stock = await GetStockAsync(request.StockId, cancellationToken);
            
            // 2. Valider qu'on peut consommer cette quantité
            ValidateConsommation(stock, request.QuantiteConsommee);
            
            // 3. Consommer (diminuer la quantité)
            stock.QuantiteActuelle -= request.QuantiteConsommee;
            stock.UpdatedAt = DateTime.Now;
            
            // 4. Sauvegarder
            await _stockRepository.UpdateAsync(stock, cancellationToken);
            
            // 5. Retourner le stock mis à jour
            return stock.ToResponseDto();
        }
        
        private async Task<Domain.Entities.Stock> GetStockAsync(
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
        
        private void ValidateConsommation(Domain.Entities.Stock stock, decimal quantite)
        {
            // Règle 1 : Quantité positive
            if (quantite <= 0)
            {
                throw new BusinessRuleException("La quantité consommée doit être positive.");
            }
            
            // Règle 2 : Stock suffisant
            if (stock.QuantiteActuelle < quantite)
            {
                throw new BusinessRuleException(
                    $"Stock insuffisant. Disponible: {stock.QuantiteActuelle} {stock.UniteMesure}, Demandé: {quantite}");
            }
            
            // Règle 3 : Stock non périmé
            if (stock.DatePeremption < DateTime.Now)
            {
                throw new BusinessRuleException(
                    "Impossible de consommer un stock périmé.");
            }
        }
    }
}