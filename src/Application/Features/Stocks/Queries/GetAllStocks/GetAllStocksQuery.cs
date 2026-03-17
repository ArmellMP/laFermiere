using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Queries.GetAllStocks
{
    /// <summary>
    /// Query pour récupérer tous les stocks.
    /// </summary>
    public record GetAllStocksQuery : IRequest<List<StockResponseDto>>
    {
        /// <summary>
        /// Filtre optionnel par type de stock. Si null, retourne tous les types.
        /// </summary>
        public string? Type { get; init; }
        
        /// <summary>
        /// Filtre optionnel par fournisseur. Si null, retourne tous les fournisseurs.
        /// </summary>
        public int? FournisseurId { get; init; }
        
        /// <summary>
        /// Inclure uniquement les stocks en alerte ?
        /// </summary>
        public bool OnlyEnAlerte { get; init; }
        
        /// <summary>
        /// Inclure uniquement les stocks non périmés ?
        /// </summary>
        public bool OnlyNonPerimes { get; init; }
    }
}