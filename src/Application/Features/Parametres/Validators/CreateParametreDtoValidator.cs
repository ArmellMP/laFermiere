using FluentValidation;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Validators
{
    public class CreateParametreDtoValidator : AbstractValidator<CreateParametreDto>
    {
        public CreateParametreDtoValidator()
        {
            RuleFor(x => x.Cle)
                .NotEmpty().WithMessage("La clé du paramètre est requise.")
                .MaximumLength(50).WithMessage("La clé ne peut dépasser 50 caractères.")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("La clé ne peut contenir que des lettres, chiffres et tirets bas.");

            RuleFor(x => x.Valeur)
                .MaximumLength(65535).WithMessage("La valeur est trop longue.")
                .When(x => !string.IsNullOrEmpty(x.Valeur));

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("La description ne peut dépasser 200 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Categorie)
                .NotEmpty().WithMessage("La catégorie est requise.")
                .Must(BeValidCategorie)
                .WithMessage("Catégorie invalide. Valeurs acceptées : Elevage, Stock, Finance, Systeme, General");
        }

        private bool BeValidCategorie(string categorie)
        {
            var validCategories = new[] { "Elevage", "Stock", "Finance", "Systeme", "General" };
            return validCategories.Contains(categorie);
        }
    }
}