using FluentValidation;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Validators
{
    public class UpdateParametreDtoValidator : AbstractValidator<UpdateParametreDto>
    {
        public UpdateParametreDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("L'ID du paramètre est requis.");

            RuleFor(x => x.Valeur)
                .MaximumLength(65535).WithMessage("La valeur est trop longue.")
                .When(x => !string.IsNullOrEmpty(x.Valeur));

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("La description ne peut dépasser 200 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Categorie)
                .Must(BeValidCategorie)
                .WithMessage("Catégorie invalide.")
                .When(x => !string.IsNullOrEmpty(x.Categorie));
        }

        private bool BeValidCategorie(string? categorie)
        {
            if (string.IsNullOrEmpty(categorie)) return true;
            var validCategories = new[] { "Elevage", "Stock", "Finance", "Systeme", "General" };
            return validCategories.Contains(categorie);
        }
    }
}