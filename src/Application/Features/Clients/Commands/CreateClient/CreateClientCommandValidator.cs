using FluentValidation;
using Application.Features.Clients.Validators;

namespace Application.Features.Clients.Commands.CreateClient
{
    /// <summary>
    /// Validateur pour la commande CreateClientCommand.
    /// </summary>
    public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
    {
        public CreateClientCommandValidator()
        {
            // Règle 1 : Le DTO ne doit pas être null
            RuleFor(x => x.CreateClientDto)
                .NotNull()
                .WithMessage("Les données du client sont requises.");

            // Règle 2 : Valider le contenu du DTO
            RuleFor(x => x.CreateClientDto)
                .SetValidator(new CreateClientDtoValidator());
        }
    }
}