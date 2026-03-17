using MediatR;
using AutoMapper;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStocksEnAlerte
{
    /// <summary>
    /// Handler pour récupérer les stocks en alerte.
    /// </summary>
    public class GetStocksEnAlerteQueryHandler : IRequestHandler<GetStocksEnAlerteQuery, List<StockResponseDto>>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public GetStocksEnAlerteQueryHandler(
            IStockRepository stockRepository,
            IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
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
            return _mapper.Map<List<StockResponseDto>>(stocks);
        }
    }
}