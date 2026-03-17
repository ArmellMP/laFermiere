using FluentValidation;

namespace Application.Features.Bandes.Commands.DeleteBande
{
    /// <summary>
    /// Validateur pour la commande DeleteBandeCommand.
    /// </summary>
    public class DeleteBandeCommandValidator : AbstractValidator<DeleteBandeCommand>
    {
        public DeleteBandeCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("L'ID de la bande doit être positif.");
        }
    }
}