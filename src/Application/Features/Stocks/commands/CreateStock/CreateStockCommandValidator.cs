using FluentValidation;
using Application.Features.Stocks.Validators;

namespace Application.Features.Stocks.Commands.CreateStock
{
    /// <summary>
    /// Validateur pour la commande CreateStockCommand..
    /// </summary>
    public class CreateStockCommandValidator : AbstractValidator<CreateStockCommand>
    {
        public CreateStockCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.CreateStockDto)
                .NotNull()
                .WithMessage("Les données du stock sont requises.");
            
            // Règle 2 : Valider le contenu du DTO avec le validateur spécifique
            RuleFor(x => x.CreateStockDto)
                .SetValidator(new CreateStockDtoValidator());
            
            // Règle 3 : Validation supplémentaire au niveau de la commande
            // Par exemple, vérifier que le Type et l'Unité de mesure sont cohérents
            RuleFor(x => x)
                .Must(HaveConsistentUnits)
                .WithMessage("L'unité de mesure n'est pas cohérente avec le type de stock.")
                .When(x => x.CreateStockDto != null);
        }
        
        /// <summary>
        /// Vérifie la cohérence entre le type de stock et l'unité de mesure.
        /// Exemple: Les aliments sont en Kg, les médicaments en ml, etc.
        /// </summary>
        private bool HaveConsistentUnits(CreateStockCommand command)
        {
            var dto = command.CreateStockDto;
            if (dto == null) return false;
            
            return dto.Type switch
            {
                "Aliment" => dto.UniteMesure == "Kg" || dto.UniteMesure == "Sac",
                "Medicament" => dto.UniteMesure == "ml" || dto.UniteMesure == "Dose",
                "Vaccin" => dto.UniteMesure == "Dose" || dto.UniteMesure == "ml",
                "Materiel" => dto.UniteMesure == "Unite" || dto.UniteMesure == "Rouleau",
                _ => true // Autre = pas de restriction
            };
        }
    }
}