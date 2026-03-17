// Application/Features/Stocks/Queries/GetStockById/GetStockByIdQueryHandler.cs
using MediatR;
using AutoMapper;
using Application.Common.Exceptions;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStockById
{
    /// <summary>
    /// Handler pour récupérer un stock par son ID.
    /// </summary>
    public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockResponseDto>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public GetStockByIdQueryHandler(
            IStockRepository stockRepository,
            IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
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
            return _mapper.Map<StockResponseDto>(stock);
        }
    }
}