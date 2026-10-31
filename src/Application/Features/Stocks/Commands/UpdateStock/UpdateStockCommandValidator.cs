using FluentValidation;
using Application.Features.Stocks.Validators;

namespace Application.Features.Stocks.Commands.UpdateStock
{
    /// <summary>
    /// Validateur pour la commande UpdateStockCommand.
    /// </summary>
    public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
    {
        public UpdateStockCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.UpdateStockDto)
                .NotNull()
                .WithMessage("Les données de mise à jour sont requises.");
            
            // Règle 2 : Valider le contenu du DTO
            RuleFor(x => x.UpdateStockDto)
                .SetValidator(new UpdateStockDtoValidator());
            
            // Règle 3 : Validation spécifique à la commande
            RuleFor(x => x.UpdateStockDto.QuantiteActuelle)
                .GreaterThanOrEqualTo(0)
                .WithMessage("La quantité ne peut pas être négative.");
        }
    }
}