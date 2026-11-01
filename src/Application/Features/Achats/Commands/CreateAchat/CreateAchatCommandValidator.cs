using FluentValidation;
using Application.Features.Achats.Validators;

namespace Application.Features.Achats.Commands.CreateAchat
{
    public class CreateAchatCommandValidator : AbstractValidator<CreateAchatCommand>
    {
        public CreateAchatCommandValidator()
        {
            RuleFor(x => x.CreateAchatDto)
                .NotNull().WithMessage("Les données de l'achat sont requises.");

            RuleFor(x => x.CreateAchatDto)
                .SetValidator(new CreateAchatDtoValidator());
        }
    }
}