using FluentValidation;
using Application.Features.Bandes.Validators; // Pour UpdateBandeDtoValidator

namespace Application.Features.Bandes.Commands.UpdateBande
{
    /// <summary>
    /// Validateur pour la commande UpdateBandeCommand.
    /// </summary>
    public class UpdateBandeCommandValidator : AbstractValidator<UpdateBandeCommand>
    {
        public UpdateBandeCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.UpdateBandeDto) 
                .NotNull()
                .WithMessage("Les données de mise à jour sont requises.");
            
            // Règle 2 : Valider le contenu du DTO avec le nouveau validateur
            RuleFor(x => x.UpdateBandeDto) 
                .SetValidator(new UpdateBandeDtoValidator()); 
            
        }
    }
}