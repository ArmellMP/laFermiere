using FluentValidation;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Validators
{
    public class UpdateAlerteDtoValidator : AbstractValidator<UpdateAlerteDto>
    {
        public UpdateAlerteDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("L'ID de l'alerte est requis.");

            RuleFor(x => x.UtilisateurId)
                .GreaterThan(0).WithMessage("L'ID de l'utilisateur est requis.")
                .When(x => x.EstLue.HasValue && x.EstLue.Value);
        }
    }
}