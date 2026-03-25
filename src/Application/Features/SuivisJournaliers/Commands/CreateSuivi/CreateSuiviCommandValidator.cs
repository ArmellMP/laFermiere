using FluentValidation;
using Application.Features.SuivisJournaliers.Validators;

namespace Application.Features.SuivisJournaliers.Commands.CreateSuivi
{
    public class CreateSuiviCommandValidator : AbstractValidator<CreateSuiviCommand>
    {
        public CreateSuiviCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.CreateSuiviDto)
                .NotNull()
                .WithMessage("Les données du suivi sont requises.");

            // Règle 2 : Valider le contenu du DTO avec notre validateur
            RuleFor(x => x.CreateSuiviDto)
                .SetValidator(new CreateSuiviDtoValidator());
        }
    }
}