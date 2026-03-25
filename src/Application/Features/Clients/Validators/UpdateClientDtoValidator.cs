using FluentValidation;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Validators
{
    /// <summary>
    /// Validateur pour la mise à jour d'un client.
    /// </summary>
    public class UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>
    {
        public UpdateClientDtoValidator()
        {
            // ============================================
            // RÈGLE 1 : L'ID est obligatoire
            // ============================================
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("L'ID du client doit être un nombre positif.");

            // ============================================
            // RÈGLE 2 : Nom (optionnel mais valide si présent)
            // ============================================
            RuleFor(x => x.Nom)
                .MaximumLength(100)
                .WithMessage("Le nom ne peut pas dépasser 100 caractères.")
                .Matches(@"^[a-zA-ZÀ-ÿ\s\-']+$")
                .WithMessage("Le nom ne peut contenir que des lettres, espaces, tirets et apostrophes.")
                .When(x => !string.IsNullOrEmpty(x.Nom));

            // ============================================
            // RÈGLE 3 : Type de client (optionnel mais valide si présent)
            // ============================================
            RuleFor(x => x.TypeClient)
                .Must(BeValidTypeClient)
                .WithMessage("Type de client invalide. Valeurs acceptées : Particulier, Restaurant, Marche, Grossiste, Transformateur, Autre.")
                .When(x => !string.IsNullOrEmpty(x.TypeClient));

            // ============================================
            // RÈGLE 4 : Téléphone (optionnel mais formaté)
            // ============================================
            RuleFor(x => x.Telephone)
                .MaximumLength(20)
                .WithMessage("Le téléphone ne peut pas dépasser 20 caractères.")
                .Matches(@"^[0-9+\-\s]+$")
                .WithMessage("Le téléphone ne peut contenir que des chiffres, +, - et espaces.")
                .When(x => !string.IsNullOrEmpty(x.Telephone));

            // ============================================
            // RÈGLE 5 : Email (optionnel mais format valide)
            // ============================================
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("L'adresse email n'est pas valide.")
                .MaximumLength(100)
                .WithMessage("L'email ne peut pas dépasser 100 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            // ============================================
            // RÈGLE 6 : Adresse (optionnelle)
            // ============================================
            RuleFor(x => x.Adresse)
                .MaximumLength(500)
                .WithMessage("L'adresse ne peut pas dépasser 500 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Adresse));

            // ============================================
            // RÈGLE 7 : Notes (optionnelles)
            // ============================================
            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .WithMessage("Les notes ne peuvent pas dépasser 2000 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            // ============================================
            // RÈGLE 8 : EstActif (optionnel, pas de validation spécifique)
            // ============================================
            // bool? n'a pas besoin de validation supplémentaire
        }

        /// <summary>
        /// Vérifie si le type de client est valide
        /// </summary>
        private bool BeValidTypeClient(string? typeClient)
        {
            if (string.IsNullOrEmpty(typeClient)) return true; // Optionnel en update
            
            var validTypes = new[] { "Particulier", "Restaurant", "Marche", "Grossiste", "Transformateur", "Autre" };
            return validTypes.Contains(typeClient);
        }
    }
}