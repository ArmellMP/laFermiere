using FluentValidation;
using Application.Features.Alertes.Validators;

namespace Application.Features.Alertes.Commands.UpdateAlerte
{
    public class UpdateAlerteCommandValidator : AbstractValidator<UpdateAlerteCommand>
    {
        public UpdateAlerteCommandValidator()
        {
            RuleFor(x => x.UpdateAlerteDto)
                .NotNull().WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateAlerteDto.Id)
                .GreaterThan(0).WithMessage("L'ID de l'alerte est requis.");

            RuleFor(x => x.UpdateAlerteDto)
                .SetValidator(new UpdateAlerteDtoValidator());
        }
    }
}