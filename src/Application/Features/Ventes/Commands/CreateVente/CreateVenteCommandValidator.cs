using FluentValidation;
using Application.Features.Ventes.Validators;

namespace Application.Features.Ventes.Commands.CreateVente
{
    public class CreateVenteCommandValidator : AbstractValidator<CreateVenteCommand>
    {
        public CreateVenteCommandValidator()
        {
            RuleFor(x => x.CreateVenteDto)
                .NotNull().WithMessage("Les données de la vente sont requises.");

            RuleFor(x => x.CreateVenteDto)
                .SetValidator(new CreateVenteDtoValidator());
        }
    }
}