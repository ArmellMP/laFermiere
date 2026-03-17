using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStocksEnAlerte
{
    /// <summary>
    /// Query pour récupérer tous les stocks en dessous du seuil d'alerte.
    /// </summary>
    public record GetStocksEnAlerteQuery : IRequest<List<StockResponseDto>>
    {
        /// <summary>
        /// Filtre optionnel par bande.
        /// </summary>
        public int? BandeId { get; init; }
    }
}