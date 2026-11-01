using FluentValidation;
using Application.Features.SuivisJournaliers.Dtos;

namespace Application.Features.SuivisJournaliers.Validators
{
    /// <summary>
    /// Validateur permettant de Vérifie que toutes les données sont valides avant d'atteindre le handler.
    /// </summary>
    public class CreateSuiviDtoValidator : AbstractValidator<CreateSuiviDto>
    {
        public CreateSuiviDtoValidator()
        {
            RuleFor(x => x.BandeId)
                .GreaterThan(0)
                .WithMessage("L'ID de la bande doit être un nombre positif.");

            RuleFor(x => x.Jour)
                .InclusiveBetween(1, 45)
                .WithMessage("Le jour doit être compris entre 1 et 45.");

            RuleFor(x => x.DateSuivi)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La date de suivi ne peut pas être dans le futur.");

            When(x => x.NbMortalites.HasValue, () =>
            {
                RuleFor(x => x.NbMortalites)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Le nombre de mortalités ne peut pas être négatif.")
                    .LessThanOrEqualTo(1000)
                    .WithMessage("Le nombre de mortalités semble trop élevé (max 1000).");

                // Si on a des mortalités, on peut avoir une cause (optionnelle)
                RuleFor(x => x.CauseMortalite)
                    .MaximumLength(200)
                    .WithMessage("La cause de mortalité ne peut pas dépasser 200 caractères.")
                    .When(x => !string.IsNullOrEmpty(x.CauseMortalite));
            });

            When(x => x.PoidsMoyen.HasValue, () =>
            {
                RuleFor(x => x.PoidsMoyen)
                    .GreaterThan(0)
                    .WithMessage("Le poids moyen doit être supérieur à 0.")
                    .LessThanOrEqualTo(10)   // en kg (décision 2026-11-01) : un poulet de chair pèse moins de 10 kg
                    .WithMessage("Le poids moyen (en kg) ne peut pas dépasser 10 kg.")
                    .PrecisionScale(6, 2, true)
                    .WithMessage("Le poids moyen ne peut pas avoir plus de 2 décimales.");

                // Si on a un poids moyen, on doit avoir le nombre de sujets pesés
                RuleFor(x => x.NbSujetsPeses)
                    .NotNull()
                    .WithMessage("Le nombre de sujets pesés est requis lorsque le poids moyen est fourni.")
                    .GreaterThan(0)
                    .WithMessage("Le nombre de sujets pesés doit être supérieur à 0.")
                    .LessThanOrEqualTo(10000)
                    .WithMessage("Le nombre de sujets pesés ne peut pas dépasser 10000.");
            });

            When(x => !string.IsNullOrEmpty(x.TypeSoin), () =>
            {
                RuleFor(x => x.TypeSoin)
                    .Must(BeValidTypeSoin)
                    .WithMessage("Type de soin invalide. Valeurs acceptées : Vaccination, Traitement, Prevention, Supplement, Autre.");

                RuleFor(x => x.ProduitUtilise)
                    .NotEmpty()
                    .WithMessage("Le nom du produit est requis pour un soin.")
                    .MaximumLength(100)
                    .WithMessage("Le nom du produit ne peut pas dépasser 100 caractères.");

                RuleFor(x => x.Dosage)
                    .MaximumLength(100)
                    .WithMessage("Le dosage ne peut pas dépasser 100 caractères.")
                    .When(x => !string.IsNullOrEmpty(x.Dosage));

                RuleFor(x => x.DureeTraitement)
                    .InclusiveBetween(1, 30)
                    .WithMessage("La durée du traitement doit être entre 1 et 30 jours.")
                    .When(x => x.DureeTraitement.HasValue);

                RuleFor(x => x.CoutSoin)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Le coût du soin ne peut pas être négatif.")
                    .LessThan(1000000)
                    .WithMessage("Le coût du soin semble trop élevé (max 1 000 000).")
                    .PrecisionScale(10, 2, true)
                    .WithMessage("Le coût ne peut pas avoir plus de 2 décimales.")
                    .When(x => x.CoutSoin.HasValue);
            });

            When(x => x.StockConsommeId.HasValue, () =>
            {
                RuleFor(x => x.StockConsommeId)
                    .GreaterThan(0)
                    .WithMessage("L'ID du stock doit être positif.");

                RuleFor(x => x.QuantiteConsommee)
                    .NotNull()
                    .WithMessage("La quantité consommée est requise lorsque le stock est spécifié.")
                    .GreaterThan(0)
                    .WithMessage("La quantité consommée doit être supérieure à 0.")
                    .LessThan(1000000)
                    .WithMessage("La quantité consommée ne peut pas dépasser 1 000 000.")
                    .PrecisionScale(10, 2, true)
                    .WithMessage("La quantité ne peut pas avoir plus de 2 décimales.");
            });

            When(x => x.Temperature.HasValue, () =>
            {
                RuleFor(x => x.Temperature)
                    .InclusiveBetween(-10, 50)
                    .WithMessage("La température doit être comprise entre -10°C et 50°C.")
                    .PrecisionScale(4, 1, true)
                    .WithMessage("La température ne peut avoir qu'une décimale.");
            });

            RuleFor(x => x.Observations)
                .MaximumLength(2000)
                .WithMessage("Les observations ne peuvent pas dépasser 2000 caractères.")
                .When(x => !string.IsNullOrEmpty(x.Observations));

            RuleFor(x => x)
                .Must(HaveConsistentSoinAndProduit)
                .WithMessage("Un type de soin doit être accompagné d'un produit.")
                .When(x => !string.IsNullOrEmpty(x.TypeSoin));
        }

        /// <summary>
        /// Vérifie si le type de soin est valide
        /// </summary>
        private bool BeValidTypeSoin(string? typeSoin)
        {
            if (string.IsNullOrEmpty(typeSoin)) return false;
            var validTypes = new[] { "Vaccination", "Traitement", "Prevention", "Supplement", "Autre" };
            return validTypes.Contains(typeSoin);
        }

        /// <summary>
        /// Vérifie la cohérence entre type de soin et produit
        /// </summary>
        private bool HaveConsistentSoinAndProduit(CreateSuiviDto dto)
        {
            // Si on a un type de soin, on doit avoir un produit
            return !string.IsNullOrEmpty(dto.ProduitUtilise);
        }
    }
}