using FluentValidation;

namespace Application.Features.Stocks.Commands.ConsommerStock
{
    /// <summary>
    /// Validateur pour la commande ConsommerStockCommand.
    /// </summary>
    public class ConsommerStockCommandValidator : AbstractValidator<ConsommerStockCommand>
    {
        public ConsommerStockCommandValidator()
        {
            RuleFor(x => x.StockId)
                .GreaterThan(0)
                .WithMessage("L'ID du stock est requis.");
            
            RuleFor(x => x.QuantiteConsommee)
                .GreaterThan(0)
                .WithMessage("La quantité consommée doit être supérieure à 0.")
                .LessThanOrEqualTo(1000000)
                .WithMessage("La quantité consommée ne peut pas dépasser 1 000 000.");
        }
    }
}