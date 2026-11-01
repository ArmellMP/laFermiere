using FluentValidation;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Validators
{
    public class UpdateUtilisateurDtoValidator : AbstractValidator<UpdateUtilisateurDto>
    {
        public UpdateUtilisateurDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("L'ID de l'utilisateur est requis.");

            RuleFor(x => x.Nom)
                .MaximumLength(100).WithMessage("Le nom ne peut dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Nom));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("L'email n'est pas valide.")
                .MaximumLength(100).WithMessage("L'email ne peut dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Le mot de passe doit contenir au moins 6 caractères.")
                .MaximumLength(100).WithMessage("Le mot de passe ne peut dépasser 100 caractères.")
                .Matches(@"[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule.")
                .Matches(@"[a-z]").WithMessage("Le mot de passe doit contenir au moins une minuscule.")
                .Matches(@"[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.")
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.Role)
                .Must(BeValidRole)
                .WithMessage("Rôle invalide. Valeurs acceptées : Admin, Fermier, Veterinaire, Comptable")
                .When(x => !string.IsNullOrEmpty(x.Role));
        }

        private bool BeValidRole(string? role)
        {
            if (string.IsNullOrEmpty(role)) return true;
            var validRoles = new[] { "Admin", "Fermier", "Veterinaire", "Comptable" };
            return validRoles.Contains(role);
        }
    }
}