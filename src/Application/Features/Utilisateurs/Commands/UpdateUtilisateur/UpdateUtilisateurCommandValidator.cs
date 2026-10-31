using FluentValidation;
using Application.Features.Utilisateurs.Validators;

namespace Application.Features.Utilisateurs.Commands.UpdateUtilisateur
{
    public class UpdateUtilisateurCommandValidator : AbstractValidator<UpdateUtilisateurCommand>
    {
        public UpdateUtilisateurCommandValidator()
        {
            RuleFor(x => x.UpdateUtilisateurDto)
                .NotNull().WithMessage("Les données de mise à jour sont requises.");

            RuleFor(x => x.UpdateUtilisateurDto.Id)
                .GreaterThan(0).WithMessage("L'ID de l'utilisateur est requis.");

            RuleFor(x => x.UpdateUtilisateurDto)
                .SetValidator(new UpdateUtilisateurDtoValidator());
        }
    }
}