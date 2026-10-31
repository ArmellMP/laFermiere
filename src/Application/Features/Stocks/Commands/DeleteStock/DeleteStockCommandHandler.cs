// Application/Features/Stocks/Commands/DeleteStock/DeleteStockCommandHandler.cs
using MediatR;
using Application.Common.Exceptions;
using Application.Features.Stocks.Interfaces;

namespace Application.Features.Stocks.Commands.DeleteStock
{
    /// <summary>
    /// Handler pour supprimer un stock.
    /// </summary>
    public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand, Unit>
    {
        private readonly IStockRepository _stockRepository;

        public DeleteStockCommandHandler(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Unit> Handle(
            DeleteStockCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Vérifier que le stock existe
            var exists = await _stockRepository.ExistsAsync(request.Id, cancellationToken);
            
            if (!exists)
            {
                throw new NotFoundException($"Stock avec l'ID {request.Id} non trouvé.");
            }
            
            // 2. Vérifier que le stock n'a pas été consommé
            var hasConsommation = await _stockRepository.HasConsommationAsync(
                request.Id, cancellationToken);
                
            if (hasConsommation)
            {
                throw new BusinessRuleException(
                    "Impossible de supprimer un stock qui a déjà été consommé.");
            }
            
            // 3. Supprimer le stock
            await _stockRepository.DeleteAsync(request.Id, cancellationToken);
            
            // 4. Retourner Unit (succès)
            return Unit.Value;
        }
    }
}