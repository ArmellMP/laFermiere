using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStockById
{
    /// <summary>
    /// Query pour récupérer un stock par son ID.
    /// </summary>
    public record GetStockByIdQuery : IRequest<StockResponseDto>
    {
        /// <summary>
        /// ID du stock à récupérer.
        /// </summary>
        public int Id { get; init; }
    }
}