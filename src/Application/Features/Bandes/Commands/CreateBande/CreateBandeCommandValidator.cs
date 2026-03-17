using FluentValidation;
using Application.Features.Bandes.Validators; // Pour CreateBandeDtoValidator

namespace Application.Features.Bandes.Commands.CreateBande
{
    /// <summary>
    /// Validateur pour la commande CreateBandeCommand.
    /// </summary>
    public class CreateBandeCommandValidator : AbstractValidator<CreateBandeCommand>
    {
        public CreateBandeCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.CreateBandeDto) 
                .NotNull()
                .WithMessage("Les données de la bande sont requises.");
            
            // Règle 2 : Valider le contenu du DTO avec le nouveau validateur
            RuleFor(x => x.CreateBandeDto) 
                .SetValidator(new CreateBandeDtoValidator()); 
        }
    }
}