using MediatR;
using AutoMapper;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStocksByBande
{
    /// <summary>
    /// Handler pour récupérer les stocks d'une bande.
    /// </summary>
    public class GetStocksByBandeQueryHandler : IRequestHandler<GetStocksByBandeQuery, List<StockResponseDto>>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public GetStocksByBandeQueryHandler(
            IStockRepository stockRepository,
            IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<List<StockResponseDto>> Handle(
            GetStocksByBandeQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer les stocks de la bande
            var stocks = await _stockRepository.GetByBandeIdAsync(request.BandeId, cancellationToken);
            
            // 2. Mapper et retourner
            return _mapper.Map<List<StockResponseDto>>(stocks);
        }
    }
}