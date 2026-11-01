// Application/Features/Bandes/Validators/UpdateBandeDtoValidator.cs
using FluentValidation;
using Application.Features.Bandes.Dtos;
using Domain.Enums;

namespace Application.Features.Bandes.Validators
{
    /// <summary>
    /// Validateur pour le DTO de mise à jour d'une bande.
    /// </summary>
    public class UpdateBandeDtoValidator : AbstractValidator<UpdateBandeDto>
    {
        public UpdateBandeDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("L'ID de la bande doit être un nombre positif");
            
            RuleFor(x => x.NbPoussinsActuel)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Le nombre actuel de poussins ne peut pas être négatif")
                .LessThanOrEqualTo(50000)   // décision 2026-11-01 : 50 000 (même limite que UpdateBandeRequest)
                .WithMessage("Le nombre actuel de poussins ne peut pas dépasser 50 000");
            
            RuleFor(x => x.Observations)
                .MaximumLength(2000)
                .WithMessage("Les observations ne peuvent pas dépasser 2000 caractères")
                .When(x => !string.IsNullOrEmpty(x.Observations));
            
            RuleFor(x => x.Statut)
                .IsInEnum()
                .WithMessage("Statut invalide");
            
            // Règle conditionnelle : si Statut = Cloturee, alors DateCloture est requise
            When(x => x.Statut == StatutBande.Cloturee, () =>
            {
                RuleFor(x => x.DateCloture)
                    .NotNull()
                    .WithMessage("La date de clôture est requise lorsque le statut est 'Clôturée'");
            });

            // Même règle que CloturerBandeCommandValidator : on clôture une bande terminée,
            // donc la date de clôture ne peut pas être dans le futur (décision 2026-11-01).
            RuleFor(x => x.DateCloture)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La date de clôture ne peut être dans le futur")
                .When(x => x.DateCloture.HasValue);
        }
    }
}