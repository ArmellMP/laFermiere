using FluentValidation;
using Application.Features.Achats.Validators;

namespace Application.Features.Achats.Commands.UpdateAchat
{
    public class UpdateAchatCommandValidator : AbstractValidator<UpdateAchatCommand>
    {
        public UpdateAchatCommandValidator()
        {
            RuleFor(x => x.UpdateAchatDto)
                .NotNull().WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateAchatDto.Id)
                .GreaterThan(0).WithMessage("L'ID de l'achat est requis.");

            RuleFor(x => x.UpdateAchatDto)
                .SetValidator(new UpdateAchatDtoValidator());
        }
    }
}