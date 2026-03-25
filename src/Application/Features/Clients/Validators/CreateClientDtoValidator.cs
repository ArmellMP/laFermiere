using FluentValidation;
using Application.Features.Clients.Dtos;
using Domain.Enums;

namespace Application.Features.Clients.Validators
{
    /// <summary>
    /// Validateur pour la création d'un client.
    /// </summary>
    public class CreateClientDtoValidator : AbstractValidator<CreateClientDto>
    {
        public CreateClientDtoValidator()
        {
            // ============================================
            // RÈGLE 1 : Nom est obligatoire
            // ============================================
            RuleFor(x => x.Nom)
                .NotEmpty()
                .WithMessage("Le nom du client est requis.")
                .MaximumLength(100)
                .WithMessage("Le nom ne peut pas dépasser 100 caractères.")
                .Matches(@"^[a-zA-ZÀ-ÿ\s\-']+$")
                .WithMessage("Le nom ne peut contenir que des lettres, espaces, tirets et apostrophes.");

            // ============================================
            // RÈGLE 2 : Type de client est obligatoire et valide
            // ============================================
            RuleFor(x => x.TypeClient)
                .NotEmpty()
                .WithMessage("Le type de client est requis.")
                .Must(BeValidTypeClient)
                .WithMessage("Type de client invalide. Valeurs acceptées : Particulier, Restaurant, Marche, Grossiste, Transformateur, Autre.");

            // ============================================
            // RÈGLE 3 : Téléphone (optionnel mais formaté)
            // ============================================
            RuleFor(x => x.Telephone)
                .MaximumLength(20)
                .WithMessage("Le téléphone ne peut pas dépasser 20 caractères.")
                .Matches(@"^[0-9+\-\s]+$")
                .WithMessage("Le téléphone ne peut contenir que des chiffres, +, - et espaces.")
                .When(x => !string.IsNullOrEmpty(x.Telephone));

            // ============================================
            // RÈGLE 4 : Email (optionnel mais format valide)
            // ============================================
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("L'adresse email n'est pas valide.")
                .MaximumLength(100)
                .WithMessage("L'email ne peut pas dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            // ============================================
            // RÈGLE 5 : Adresse (optionnelle)
            // ============================================
            RuleFor(x => x.Adresse)
                .MaximumLength(500)
                .WithMessage("L'adresse ne peut pas dépasser 500 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Adresse));

            // ============================================
            // RÈGLE 6 : Notes (optionnelles)
            // ============================================
            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .WithMessage("Les notes ne peuvent pas dépasser 2000 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        /// <summary>
        /// Vérifie si le type de client est valide
        /// </summary>
        private bool BeValidTypeClient(string typeClient)
        {
            var validTypes = new[] { "Particulier", "Restaurant", "Marche", "Grossiste", "Transformateur", "Autre" };
            return validTypes.Contains(typeClient);
        }
    }
}