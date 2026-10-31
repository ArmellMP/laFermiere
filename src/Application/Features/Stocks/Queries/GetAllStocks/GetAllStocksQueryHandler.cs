using MediatR;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;
using Application.Features.Stocks.Queries.GetStocksPerimes;
using Application.Common.Mappings;

namespace Application.Features.Stocks.Queries.GetAllStocks
{
    /// <summary>
    /// Handler pour récupérer tous les stocks avec filtres optionnels.
    /// </summary>
    public class GetAllStocksQueryHandler : IRequestHandler<GetAllStocksQuery, List<StockResponseDto>>
    {
        private readonly IStockRepository _stockRepository;

        public GetAllStocksQueryHandler(
            IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<List<StockResponseDto>> Handle(
            GetAllStocksQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer tous les stocks
            var stocks = await _stockRepository.GetAllAsync(cancellationToken);
            
            // 2. Appliquer les filtres si nécessaire
            if (request.Type != null)
            {
                stocks = stocks.Where(s => s.Type.ToString() == request.Type);
            }
            
            if (request.FournisseurId.HasValue)
            {
                stocks = stocks.Where(s => s.FournisseurId == request.FournisseurId);
            }
            
            if (request.OnlyEnAlerte)
            {
                // ✅ Correct : EstEnAlerte est une PROPRIÉTÉ CALCULÉE de l'entité Stock
                stocks = stocks.Where(s => s.EstEnAlerte);
            }
            
            if (request.OnlyNonPerimes)
            {
                // ✅ CORRECTION ICI : Remplacer EstPerime par une condition sur DatePeremption
                stocks = stocks.Where(s => !s.DatePeremption.HasValue || s.DatePeremption > DateTime.Now);
            }
            
            // 3. Mapper et retourner
            return stocks.ToResponseDtos();
        }
    }
}