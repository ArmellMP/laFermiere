using FluentValidation;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Validators
{
    public class CreateAchatDtoValidator : AbstractValidator<CreateAchatDto>
    {
        public CreateAchatDtoValidator()
        {
            RuleFor(x => x.FournisseurId)
                .GreaterThan(0).WithMessage("L'ID du fournisseur est requis.");

            RuleFor(x => x.BandeId)
                .GreaterThan(0).WithMessage("L'ID de la bande est requis.");

            RuleFor(x => x.DateAchat)
                .NotEmpty().WithMessage("La date d'achat est requise.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("La date d'achat ne peut pas être dans le futur.");

            RuleFor(x => x.TypeAchat)
                .NotEmpty().WithMessage("Le type d'achat est requis.")
                .Must(BeValidTypeAchat)
                .WithMessage("Type d'achat invalide. Valeurs acceptées : Poussins, Aliment, Medicament, Materiel, Services, Transport, Autre");

            RuleFor(x => x.Quantite)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("La quantité ne peut dépasser 1 000 000.");

            RuleFor(x => x.UniteMesure)
                .NotEmpty().WithMessage("L'unité de mesure est requise.")
                .Must(BeValidUniteMesure)
                .WithMessage("Unité de mesure invalide. Valeurs acceptées : Unite, Kg, L, Sac, Dose, Heure, Km, Paquet");

            RuleFor(x => x.PrixUnitaire)
                .GreaterThan(0).WithMessage("Le prix unitaire doit être supérieur à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Le prix unitaire ne peut dépasser 1 000 000.");

            RuleFor(x => x.FactureNumero)
                .MaximumLength(50).WithMessage("Le numéro de facture ne peut dépasser 50 caractères.")
                .When(x => !string.IsNullOrEmpty(x.FactureNumero));

            RuleFor(x => x.DateReception)
                .GreaterThanOrEqualTo(x => x.DateAchat)
                .WithMessage("La date de réception ne peut pas être antérieure à la date d'achat.")
                .When(x => x.DateReception.HasValue);

            RuleFor(x => x.Observations)
                .MaximumLength(2000).WithMessage("Les observations ne peuvent dépasser 2000 caractères.");
        }

        private bool BeValidTypeAchat(string type)
        {
            var validTypes = new[] { "Poussins", "Aliment", "Medicament", "Materiel", "Services", "Transport", "Autre" };
            return validTypes.Contains(type);
        }

        private bool BeValidUniteMesure(string unite)
        {
            var validUnits = new[] { "Unite", "Kg", "L", "Sac", "Dose", "Heure", "Km", "Paquet" };
            return validUnits.Contains(unite);
        }
    }
}