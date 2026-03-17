using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetStocksPerimes
{
    /// <summary>
    /// Query pour récupérer tous les stocks périmés.
    /// </summary>
    public record GetStocksPerimesQuery : IRequest<List<StockResponseDto>>
    {
        /// <summary>
        /// Inclure les stocks qui périment dans les X prochains jours.
        /// </summary>
        public int? JoursAlerte { get; init; }
    }
}