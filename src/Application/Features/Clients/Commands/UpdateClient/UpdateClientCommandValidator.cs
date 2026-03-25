using FluentValidation;
using Application.Features.Clients.Validators;

namespace Application.Features.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
    {
        public UpdateClientCommandValidator()
        {
            RuleFor(x => x.UpdateClientDto)
                .NotNull()
                .WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateClientDto.Id)
                .GreaterThan(0)
                .WithMessage("L'ID du client est requis.");

            RuleFor(x => x.UpdateClientDto)
                .SetValidator(new UpdateClientDtoValidator());
        }
    }
}