using FluentValidation;
using Application.Features.Parametres.Validators;

namespace Application.Features.Parametres.Commands.CreateParametre
{
    public class CreateParametreCommandValidator : AbstractValidator<CreateParametreCommand>
    {
        public CreateParametreCommandValidator()
        {
            RuleFor(x => x.CreateParametreDto)
                .NotNull().WithMessage("Les données du paramètre sont requises.");

            RuleFor(x => x.CreateParametreDto)
                .SetValidator(new CreateParametreDtoValidator());
        }
    }
}