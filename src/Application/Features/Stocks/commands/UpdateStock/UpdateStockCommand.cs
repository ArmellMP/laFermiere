using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Commands.UpdateStock
{
    /// <summary>
    /// Commande pour mettre à jour un stock existant.
    /// </summary>
    public record UpdateStockCommand : IRequest<StockResponseDto>
    {
        /// <summary>
        /// Données de mise à jour du stock.
        /// </summary>
        public UpdateStockDto UpdateStockDto { get; init; } = null!;
    }
}