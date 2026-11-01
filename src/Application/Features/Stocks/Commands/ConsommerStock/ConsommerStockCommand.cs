using MediatR;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Commands.ConsommerStock
{
    /// <summary>
    /// Commande pour consommer une quantité d'un stock.
    /// Action métier spécifique (pas un simple update).
    /// </summary>
    public record ConsommerStockCommand : IRequest<StockResponseDto>
    {
        /// <summary>
        /// ID du stock à consommer.
        /// </summary>
        public int StockId { get; init; }
        
        /// <summary>
        /// Quantité consommée.
        /// </summary>
        public decimal QuantiteConsommee { get; init; }
        
        /// <summary>
        /// ID du suivi journalier associé (optionnel).
        /// </summary>
        public int? SuiviJournalierId { get; init; }
    }
}