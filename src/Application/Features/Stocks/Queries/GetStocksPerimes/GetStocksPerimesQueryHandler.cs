using MediatR;
using AutoMapper;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStocksPerimes
{
    /// <summary>
    /// Handler pour récupérer les stocks périmés.
    /// </summary>
    public class GetStocksPerimesQueryHandler : IRequestHandler<GetStocksPerimesQuery, List<StockResponseDto>>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public GetStocksPerimesQueryHandler(
            IStockRepository stockRepository,
            IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<List<StockResponseDto>> Handle(
            GetStocksPerimesQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer tous les stocks
            var stocks = await _stockRepository.GetAllAsync(cancellationToken);
            
            // 2. Filtrer les périmés
            var now = DateTime.Now;
            var stocksPerimes = stocks.Where(s => s.DatePeremption < now);
            
            // 3. Si JoursAlerte est spécifié, inclure ceux qui périment bientôt
            if (request.JoursAlerte.HasValue)
            {
                var dateLimite = now.AddDays(request.JoursAlerte.Value);
                stocksPerimes = stocks.Where(s => 
                    s.DatePeremption < dateLimite); // Périmés OU bientôt périmés
            }
            
            // 4. Mapper et retourner
            return _mapper.Map<List<StockResponseDto>>(stocksPerimes);
        }
    }
}