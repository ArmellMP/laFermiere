using FluentValidation;
using Application.Features.Alertes.Validators;

namespace Application.Features.Alertes.Commands.CreateAlerte
{
    public class CreateAlerteCommandValidator : AbstractValidator<CreateAlerteCommand>
    {
        public CreateAlerteCommandValidator()
        {
            RuleFor(x => x.CreateAlerteDto)
                .NotNull().WithMessage("Les données de l'alerte sont requises.");

            RuleFor(x => x.CreateAlerteDto)
                .SetValidator(new CreateAlerteDtoValidator());
        }
    }
}