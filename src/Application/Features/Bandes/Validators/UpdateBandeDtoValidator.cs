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
                .LessThanOrEqualTo(10000)
                .WithMessage("Le nombre actuel de poussins ne peut pas dépasser 10 000");
            
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
        }
    }
}