using FluentValidation;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Validators
{
    public class CreateVenteDtoValidator : AbstractValidator<CreateVenteDto>
    {
        public CreateVenteDtoValidator()
        {
            RuleFor(x => x.BandeId)
                .GreaterThan(0).WithMessage("L'ID de la bande est requis.");

            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("L'ID du client est requis.");

            RuleFor(x => x.DateVente)
                .NotEmpty().WithMessage("La date de vente est requise.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("La date de vente ne peut pas être dans le futur.");

            RuleFor(x => x.TypeVente)
                .NotEmpty().WithMessage("Le type de vente est requis.")
                .Must(BeValidTypeVente)
                .WithMessage("Type de vente invalide. Valeurs acceptées : PouletsVivants, PouletsNettoyes, PouletsFumes, Fientes, Autres");

            RuleFor(x => x.Quantite)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("La quantité ne peut dépasser 1 000 000.");

            RuleFor(x => x.UniteMesure)
                .NotEmpty().WithMessage("L'unité de mesure est requise.")
                .Must(BeValidUniteMesure)
                .WithMessage("Unité de mesure invalide. Valeurs acceptées : Unite, Kg, Sac, Camion, Rouleau, Paquet");

            RuleFor(x => x.PrixUnitaire)
                .GreaterThan(0).WithMessage("Le prix unitaire doit être supérieur à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Le prix unitaire ne peut dépasser 1 000 000.");

            RuleFor(x => x.ModePaiement)
                .NotEmpty().WithMessage("Le mode de paiement est requis.")
                .Must(BeValidModePaiement)
                .WithMessage("Mode de paiement invalide. Valeurs acceptées : Especes, PaiementMobile, VirementBancaire, Cheque, Credit");

            // Règle conditionnelle : Si EstPaye est true, DatePaiement est obligatoire
            When(x => x.EstPaye, () =>
            {
                RuleFor(x => x.DatePaiement)
                    .NotNull().WithMessage("La date de paiement est requise lorsque le paiement est effectué.")
                    .LessThanOrEqualTo(DateTime.Now).WithMessage("La date de paiement ne peut pas être dans le futur.");
            });

            RuleFor(x => x.Observations)
                .MaximumLength(2000).WithMessage("Les observations ne peuvent dépasser 2000 caractères.");
        }

        private bool BeValidTypeVente(string type)
        {
            var validTypes = new[] { "PouletsVivants", "PouletsNettoyes", "PouletsFumes", "Fientes", "Autres" };
            return validTypes.Contains(type);
        }

        private bool BeValidUniteMesure(string unite)
        {
            var validUnits = new[] { "Unite", "Kg", "Sac", "Camion", "Rouleau", "Paquet" };
            return validUnits.Contains(unite);
        }

        private bool BeValidModePaiement(string mode)
        {
            var validModes = new[] { "Especes", "PaiementMobile", "VirementBancaire", "Cheque", "Credit" };
            return validModes.Contains(mode);
        }
    }
}