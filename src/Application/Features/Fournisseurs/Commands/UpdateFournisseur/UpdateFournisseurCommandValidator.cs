using FluentValidation;
using Application.Features.Fournisseurs.Validators;

namespace Application.Features.Fournisseurs.Commands.UpdateFournisseur
{
    public class UpdateFournisseurCommandValidator : AbstractValidator<UpdateFournisseurCommand>
    {
        public UpdateFournisseurCommandValidator()
        {
            RuleFor(x => x.UpdateFournisseurDto)
                .NotNull().WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateFournisseurDto.Id)
                .GreaterThan(0).WithMessage("L'ID du fournisseur est requis.");

            RuleFor(x => x.UpdateFournisseurDto)
                .SetValidator(new UpdateFournisseurDtoValidator());
        }
    }
}