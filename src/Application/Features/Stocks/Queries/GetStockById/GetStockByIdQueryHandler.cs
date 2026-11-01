// Application/Features/Stocks/Queries/GetStockById/GetStockByIdQueryHandler.cs
using MediatR;
using Application.Common.Exceptions;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Stocks.Queries.GetStockById
{
    /// <summary>
    /// Handler pour récupérer un stock par son ID.
    /// </summary>
    public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockResponseDto>
    {
        private readonly IStockRepository _stockRepository;

        public GetStockByIdQueryHandler(
            IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<StockResponseDto> Handle(
            GetStockByIdQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer le stock
            var stock = await _stockRepository.GetByIdAsync(request.Id, cancellationToken);
            
            // 2. Vérifier qu'il existe
            if (stock == null)
            {
                throw new NotFoundException($"Stock avec l'ID {request.Id} non trouvé.");
            }
            
            // 3. Mapper et retourner
            return stock.ToResponseDto();
        }
    }
}