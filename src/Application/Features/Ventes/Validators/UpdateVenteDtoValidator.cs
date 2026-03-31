using FluentValidation;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Validators
{
    public class UpdateVenteDtoValidator : AbstractValidator<UpdateVenteDto>
    {
        public UpdateVenteDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("L'ID de la vente est requis.");

            RuleFor(x => x.DateVente)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("La date de vente ne peut pas être dans le futur.")
                .When(x => x.DateVente.HasValue);

            RuleFor(x => x.TypeVente)
                .Must(BeValidTypeVente)
                .WithMessage("Type de vente invalide. Valeurs acceptées : PouletsVivants, PouletsNettoyes, PouletsFumes, Fientes, Autres")
                .When(x => !string.IsNullOrEmpty(x.TypeVente));

            RuleFor(x => x.Quantite)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("La quantité ne peut dépasser 1 000 000.")
                .When(x => x.Quantite.HasValue);

            RuleFor(x => x.UniteMesure)
                .Must(BeValidUniteMesure)
                .WithMessage("Unité de mesure invalide. Valeurs acceptées : Unite, Kg, Sac, Camion, Rouleau, Paquet")
                .When(x => !string.IsNullOrEmpty(x.UniteMesure));

            RuleFor(x => x.PrixUnitaire)
                .GreaterThan(0).WithMessage("Le prix unitaire doit être supérieur à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Le prix unitaire ne peut dépasser 1 000 000.")
                .When(x => x.PrixUnitaire.HasValue);

            RuleFor(x => x.ModePaiement)
                .Must(BeValidModePaiement)
                .WithMessage("Mode de paiement invalide. Valeurs acceptées : Especes, PaiementMobile, VirementBancaire, Cheque, Credit")
                .When(x => !string.IsNullOrEmpty(x.ModePaiement));
        }

        private bool BeValidTypeVente(string? type)
        {
            if (string.IsNullOrEmpty(type)) return true;
            var validTypes = new[] { "PouletsVivants", "PouletsNettoyes", "PouletsFumes", "Fientes", "Autres" };
            return validTypes.Contains(type);
        }

        private bool BeValidUniteMesure(string? unite)
        {
            if (string.IsNullOrEmpty(unite)) return true;
            var validUnits = new[] { "Unite", "Kg", "Sac", "Camion", "Rouleau", "Paquet" };
            return validUnits.Contains(unite);
        }

        private bool BeValidModePaiement(string? mode)
        {
            if (string.IsNullOrEmpty(mode)) return true;
            var validModes = new[] { "Especes", "PaiementMobile", "VirementBancaire", "Cheque", "Credit" };
            return validModes.Contains(mode);
        }
    }
}