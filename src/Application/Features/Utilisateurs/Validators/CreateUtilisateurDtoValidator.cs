using FluentValidation;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Validators
{
    public class CreateUtilisateurDtoValidator : AbstractValidator<CreateUtilisateurDto>
    {
        public CreateUtilisateurDtoValidator()
        {
            RuleFor(x => x.Nom)
                .NotEmpty().WithMessage("Le nom est requis.")
                .MaximumLength(100).WithMessage("Le nom ne peut dépasser 100 caractères.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("L'email est requis.")
                .EmailAddress().WithMessage("L'email n'est pas valide.")
                .MaximumLength(100).WithMessage("L'email ne peut dépasser 100 caractères.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Le mot de passe est requis.")
                .MinimumLength(6).WithMessage("Le mot de passe doit contenir au moins 6 caractères.")
                .MaximumLength(100).WithMessage("Le mot de passe ne peut dépasser 100 caractères.")
                .Matches(@"[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule.")
                .Matches(@"[a-z]").WithMessage("Le mot de passe doit contenir au moins une minuscule.")
                .Matches(@"[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Le rôle est requis.")
                .Must(BeValidRole)
                .WithMessage("Rôle invalide. Valeurs acceptées : Admin, Fermier, Veterinaire, Comptable");
        }

        private bool BeValidRole(string role)
        {
            var validRoles = new[] { "Admin", "Fermier", "Veterinaire", "Comptable" };
            return validRoles.Contains(role);
        }
    }
}