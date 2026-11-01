using MediatR;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Stocks.Queries.GetStocksByBande
{
    /// <summary>
    /// Handler pour récupérer les stocks d'une bande.
    /// </summary>
    public class GetStocksByBandeQueryHandler : IRequestHandler<GetStocksByBandeQuery, List<StockResponseDto>>
    {
        private readonly IStockRepository _stockRepository;

        public GetStocksByBandeQueryHandler(
            IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<List<StockResponseDto>> Handle(
            GetStocksByBandeQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer les stocks de la bande
            var stocks = await _stockRepository.GetByBandeIdAsync(request.BandeId, cancellationToken);
            
            // 2. Mapper et retourner
            return stocks.ToResponseDtos();
        }
    }
}