using FluentValidation;
using Application.Features.Utilisateurs.Validators;

namespace Application.Features.Utilisateurs.Commands.CreateUtilisateur
{
    public class CreateUtilisateurCommandValidator : AbstractValidator<CreateUtilisateurCommand>
    {
        public CreateUtilisateurCommandValidator()
        {
            RuleFor(x => x.CreateUtilisateurDto)
                .NotNull().WithMessage("Les données de l'utilisateur sont requises.");

            RuleFor(x => x.CreateUtilisateurDto)
                .SetValidator(new CreateUtilisateurDtoValidator());
        }
    }
}