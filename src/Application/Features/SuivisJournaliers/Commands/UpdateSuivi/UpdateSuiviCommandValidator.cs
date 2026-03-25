using FluentValidation;
using Application.Features.SuivisJournaliers.Validators;

namespace Application.Features.SuivisJournaliers.Commands.UpdateSuivi
{
    public class UpdateSuiviCommandValidator : AbstractValidator<UpdateSuiviCommand>
    {
        public UpdateSuiviCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.UpdateSuiviDto)
                .NotNull()
                .WithMessage("Les données de mise à jour sont requises.");

            // Règle 2 : L'ID est obligatoire
            RuleFor(x => x.UpdateSuiviDto.Id)
                .GreaterThan(0)
                .WithMessage("L'ID du suivi est requis.");

            // Règle 3 : Valider le contenu du DTO
            RuleFor(x => x.UpdateSuiviDto)
                .SetValidator(new UpdateSuiviDtoValidator());
        }
    }
}