using FluentValidation;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Validators
{
    public class CreateAlerteDtoValidator : AbstractValidator<CreateAlerteDto>
    {
        public CreateAlerteDtoValidator()
        {
            RuleFor(x => x.TypeAlerte)
                .NotEmpty().WithMessage("Le type d'alerte est requis.")
                .Must(BeValidTypeAlerte)
                .WithMessage("Type d'alerte invalide. Valeurs acceptées : StockFaible, Peremption, MortaliteElevee, Temperature, PeseeManquante, BandeACloturer");

            RuleFor(x => x.EntiteId)
                .GreaterThan(0).WithMessage("L'ID de l'entité est requis.");

            RuleFor(x => x.Niveau)
                .NotEmpty().WithMessage("Le niveau d'alerte est requis.")
                .Must(BeValidNiveau)
                .WithMessage("Niveau invalide. Valeurs acceptées : Info, Avertissement, Critique");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Le message est requis.")
                .MaximumLength(65535).WithMessage("Le message est trop long.");
        }

        private bool BeValidTypeAlerte(string type)
        {
            var validTypes = new[] { "StockFaible", "Peremption", "MortaliteElevee", "Temperature", "PeseeManquante", "BandeACloturer" };
            return validTypes.Contains(type);
        }

        private bool BeValidNiveau(string niveau)
        {
            var validNiveaux = new[] { "Info", "Avertissement", "Critique" };
            return validNiveaux.Contains(niveau);
        }
    }
}