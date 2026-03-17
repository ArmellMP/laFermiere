// Application/Features/Stocks/Commands/DeleteStock/DeleteStockCommand.cs
using MediatR;

namespace Application.Features.Stocks.Commands.DeleteStock
{
    /// <summary>
    /// Commande pour supprimer un stock.
    /// </summary>
    public record DeleteStockCommand : IRequest<Unit>  // Unit = retour vide (comme void)
    {
        /// <summary>
        /// ID du stock à supprimer.
        /// </summary>
        public int Id { get; init; }
    }
}