using FluentValidation;
using Application.Features.Parametres.Validators;

namespace Application.Features.Parametres.Commands.UpdateParametre
{
    public class UpdateParametreCommandValidator : AbstractValidator<UpdateParametreCommand>
    {
        public UpdateParametreCommandValidator()
        {
            RuleFor(x => x.UpdateParametreDto)
                .NotNull().WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateParametreDto.Id)
                .GreaterThan(0).WithMessage("L'ID du paramètre est requis.");

            RuleFor(x => x.UpdateParametreDto)
                .SetValidator(new UpdateParametreDtoValidator());
        }
    }
}