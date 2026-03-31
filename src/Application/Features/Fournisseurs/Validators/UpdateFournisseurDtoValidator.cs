using FluentValidation;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Validators
{
    public class UpdateFournisseurDtoValidator : AbstractValidator<UpdateFournisseurDto>
    {
        public UpdateFournisseurDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("L'ID du fournisseur doit être positif.");

            RuleFor(x => x.Nom)
                .MaximumLength(100).WithMessage("Le nom ne peut dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Nom));

            RuleFor(x => x.TypeFournisseur)
                .Must(BeValidTypeFournisseur)
                .WithMessage("Type de fournisseur invalide.")
                .When(x => !string.IsNullOrEmpty(x.TypeFournisseur));

            RuleFor(x => x.Telephone)
                .MaximumLength(20).WithMessage("Le téléphone ne peut dépasser 20 caractères.")
                .Matches(@"^[0-9+\-\s]+$").WithMessage("Format de téléphone invalide.")
                .When(x => !string.IsNullOrEmpty(x.Telephone));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("L'adresse email n'est pas valide.")
                .MaximumLength(100).WithMessage("L'email ne peut dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.ContactPersonne)
                .MaximumLength(100).WithMessage("Le nom du contact ne peut dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.ContactPersonne));

            RuleFor(x => x.Adresse)
                .MaximumLength(500).WithMessage("L'adresse ne peut dépasser 500 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Adresse));

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Les notes ne peuvent dépasser 2000 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        private bool BeValidTypeFournisseur(string? type)
        {
            if (string.IsNullOrEmpty(type)) return true;
            var validTypes = new[] { "Poussiniere", "Aliment", "Medicament", "Materiel", "Services", "Transport", "Autre" };
            return validTypes.Contains(type);
        }
    }
}