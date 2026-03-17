using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStocksByBande
{
    /// <summary>
    /// Query pour récupérer tous les stocks d'une bande spécifique.
    /// </summary>
    public record GetStocksByBandeQuery : IRequest<List<StockResponseDto>>
    {
        /// <summary>
        /// ID de la bande dont on veut les stocks.
        /// </summary>
        public int BandeId { get; init; }
    }
}