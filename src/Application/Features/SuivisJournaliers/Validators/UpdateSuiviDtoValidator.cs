using FluentValidation;
using Application.Features.SuivisJournaliers.Dtos;

namespace Application.Features.SuivisJournaliers.Validators
{
    /// <summary>
    /// Validateur pour la mise à jour d'un suivi journalier.
    /// </summary>
    public class UpdateSuiviDtoValidator : AbstractValidator<UpdateSuiviDto>
    {
        public UpdateSuiviDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("L'ID du suivi doit être un nombre positif.");

            When(x => x.NbMortalites.HasValue, () =>
            {
                RuleFor(x => x.NbMortalites)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Le nombre de mortalités ne peut pas être négatif.");

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
                    .LessThan(10000)
                    .WithMessage("Le poids moyen semble trop élevé.")
                    .PrecisionScale(6, 2, true);

                RuleFor(x => x.NbSujetsPeses)
                    .NotNull()
                    .WithMessage("Le nombre de sujets pesés est requis.")
                    .GreaterThan(0)
                    .WithMessage("Le nombre de sujets pesés doit être supérieur à 0.");
            });

            When(x => !string.IsNullOrEmpty(x.TypeSoin), () =>
            {
                RuleFor(x => x.TypeSoin)
                    .Must(BeValidTypeSoin)
                    .WithMessage("Type de soin invalide.");

                RuleFor(x => x.ProduitUtilise)
                    .NotEmpty()
                    .WithMessage("Le nom du produit est requis.")
                    .MaximumLength(100);

                RuleFor(x => x.Dosage)
                    .MaximumLength(100)
                    .When(x => !string.IsNullOrEmpty(x.Dosage));

                RuleFor(x => x.DureeTraitement)
                    .InclusiveBetween(1, 30)
                    .When(x => x.DureeTraitement.HasValue);

                RuleFor(x => x.CoutSoin)
                    .GreaterThanOrEqualTo(0)
                    .LessThan(1000000)
                    .PrecisionScale(10, 2, true)
                    .When(x => x.CoutSoin.HasValue);
            });

            When(x => x.StockConsommeId.HasValue, () =>
            {
                RuleFor(x => x.StockConsommeId)
                    .GreaterThan(0);

                RuleFor(x => x.QuantiteConsommee)
                    .NotNull()
                    .GreaterThan(0)
                    .LessThan(1000000)
                    .PrecisionScale(10, 2, true);
            });

            When(x => x.Temperature.HasValue, () =>
            {
                RuleFor(x => x.Temperature)
                    .InclusiveBetween(-10, 50)
                    .PrecisionScale(4, 1, true);
            });

            RuleFor(x => x.Observations)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.Observations));
        }

        private bool BeValidTypeSoin(string? typeSoin)
        {
            if (string.IsNullOrEmpty(typeSoin)) return false;
            var validTypes = new[] { "Vaccination", "Traitement", "Prevention", "Supplement", "Autre" };
            return validTypes.Contains(typeSoin);
        }
    }
}