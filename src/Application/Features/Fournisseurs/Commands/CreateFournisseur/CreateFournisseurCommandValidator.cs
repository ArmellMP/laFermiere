using FluentValidation;
using Application.Features.Fournisseurs.Validators;

namespace Application.Features.Fournisseurs.Commands.CreateFournisseur
{
    public class CreateFournisseurCommandValidator : AbstractValidator<CreateFournisseurCommand>
    {
        public CreateFournisseurCommandValidator()
        {
            RuleFor(x => x.CreateFournisseurDto)
                .NotNull().WithMessage("Les données du fournisseur sont requises.");

            RuleFor(x => x.CreateFournisseurDto)
                .SetValidator(new CreateFournisseurDtoValidator());
        }
    }
}