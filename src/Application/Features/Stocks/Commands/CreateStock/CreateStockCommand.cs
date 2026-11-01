using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Commands.CreateStock
{
    /// <summary>
    /// Commande pour créer un nouveau stock.
    /// </summary>
    public record CreateStockCommand : IRequest<StockResponseDto>
    {
        /// <summary>
        /// Données de création du stock.
        /// </summary>
        public CreateStockDto CreateStockDto { get; init; } = null!;
    }
}