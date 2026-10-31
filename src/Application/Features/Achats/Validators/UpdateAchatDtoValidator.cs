using FluentValidation;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Validators
{
    public class UpdateAchatDtoValidator : AbstractValidator<UpdateAchatDto>
    {
        public UpdateAchatDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("L'ID de l'achat est requis.");

            RuleFor(x => x.DateAchat)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("La date d'achat ne peut pas être dans le futur.")
                .When(x => x.DateAchat.HasValue);

            RuleFor(x => x.TypeAchat)
                .Must(BeValidTypeAchat)
                .WithMessage("Type d'achat invalide.")
                .When(x => !string.IsNullOrEmpty(x.TypeAchat));

            RuleFor(x => x.Quantite)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("La quantité ne peut dépasser 1 000 000.")
                .When(x => x.Quantite.HasValue);

            RuleFor(x => x.UniteMesure)
                .Must(BeValidUniteMesure)
                .WithMessage("Unité de mesure invalide.")
                .When(x => !string.IsNullOrEmpty(x.UniteMesure));

            RuleFor(x => x.PrixUnitaire)
                .GreaterThan(0).WithMessage("Le prix unitaire doit être supérieur à 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Le prix unitaire ne peut dépasser 1 000 000.")
                .When(x => x.PrixUnitaire.HasValue);

            RuleFor(x => x.FactureNumero)
                .MaximumLength(50).WithMessage("Le numéro de facture ne peut dépasser 50 caractères.")
                .When(x => !string.IsNullOrEmpty(x.FactureNumero));

            RuleFor(x => x.DateReception)
                .GreaterThanOrEqualTo(x => x.DateAchat)
                .WithMessage("La date de réception ne peut pas être antérieure à la date d'achat.")
                .When(x => x.DateReception.HasValue && x.DateAchat.HasValue);
        }

        private bool BeValidTypeAchat(string? type)
        {
            if (string.IsNullOrEmpty(type)) return true;
            var validTypes = new[] { "Poussins", "Aliment", "Medicament", "Materiel", "Services", "Transport", "Autre" };
            return validTypes.Contains(type);
        }

        private bool BeValidUniteMesure(string? unite)
        {
            if (string.IsNullOrEmpty(unite)) return true;
            var validUnits = new[] { "Unite", "Kg", "L", "Sac", "Dose", "Heure", "Km", "Paquet" };
            return validUnits.Contains(unite);
        }
    }
}