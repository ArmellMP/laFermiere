using MediatR;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Stocks.Queries.GetStocksEnAlerte
{
    /// <summary>
    /// Handler pour récupérer les stocks en alerte.
    /// </summary>
    public class GetStocksEnAlerteQueryHandler : IRequestHandler<GetStocksEnAlerteQuery, List<StockResponseDto>>
    {
        private readonly IStockRepository _stockRepository;

        public GetStocksEnAlerteQueryHandler(
            IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<List<StockResponseDto>> Handle(
            GetStocksEnAlerteQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer tous les stocks en alerte
            var stocks = await _stockRepository.GetEnAlerteAsync(cancellationToken);
            
            // 2. Filtrer par bande si demandé
            if (request.BandeId.HasValue)
            {
                stocks = stocks.Where(s => s.BandeId == request.BandeId);
            }
            
            // 3. Mapper et retourner
            return stocks.ToResponseDtos();
        }
    }
}