using FluentValidation;
using Application.Features.Ventes.Validators;

namespace Application.Features.Ventes.Commands.UpdateVente
{
    public class UpdateVenteCommandValidator : AbstractValidator<UpdateVenteCommand>
    {
        public UpdateVenteCommandValidator()
        {
            RuleFor(x => x.UpdateVenteDto)
                .NotNull().WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateVenteDto.Id)
                .GreaterThan(0).WithMessage("L'ID de la vente est requis.");

            RuleFor(x => x.UpdateVenteDto)
                .SetValidator(new UpdateVenteDtoValidator());
        }
    }
}