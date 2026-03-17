using FluentValidation;

namespace Application.Features.Bandes.Commands.CloturerBande
{
    /// <summary>
    /// Validateur pour la commande CloturerBandeCommand.
    /// </summary>
    public class CloturerBandeCommandValidator : AbstractValidator<CloturerBandeCommand>
    {
        public CloturerBandeCommandValidator()
        {
            // Règle 1 : ID requis
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("L'ID de la bande doit être positif.");
            
            // Règle 2 : DateCloture ne peut être future
            RuleFor(x => x.DateCloture)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La date de clôture ne peut être dans le futur")
                .When(x => x.DateCloture.HasValue);
            
            // Règle 3 : Observations longueur maximale
            RuleFor(x => x.ObservationsCloture)
                .MaximumLength(2000)
                .WithMessage("Les observations ne peuvent dépasser 2000 caractères")
                .When(x => !string.IsNullOrEmpty(x.ObservationsCloture));
        }
    }
}