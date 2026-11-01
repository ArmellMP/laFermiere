// Application/Features/Bandes/Validators/CreateBandeDtoValidator.cs
using FluentValidation;
using Application.Features.Bandes.Dtos;

namespace Application.Features.Bandes.Validators
{
    /// <summary>
    /// Validateur pour le DTO de création d'une bande.
    /// </summary>
    public class CreateBandeDtoValidator : AbstractValidator<CreateBandeDto>
    {
        public CreateBandeDtoValidator()
        {
            RuleFor(x => x.DateArrivee)
                .NotEmpty()
                .WithMessage("La date d'arrivée est requise")
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La date d'arrivée ne peut pas être dans le futur");
            
            RuleFor(x => x.NbPoussinsInit)
                .GreaterThan(0)
                .WithMessage("Le nombre initial de poussins doit être supérieur à 0")
                .LessThanOrEqualTo(50000)   // décision 2026-11-01 : 50 000 (même limite que CreateBandeRequest)
                .WithMessage("Le nombre initial de poussins ne peut pas dépasser 50 000");
            
            RuleFor(x => x.Observations)
                .MaximumLength(2000)
                .WithMessage("Les observations ne peuvent pas dépasser 2000 caractères")
                .When(x => !string.IsNullOrEmpty(x.Observations));
            
            RuleFor(x => x.Code)
                .MaximumLength(50)
                .WithMessage("Le code ne peut pas dépasser 50 caractères")
                .Matches(@"^\d{4}-\d{2}-\d{2}-\d{3}$")
                .WithMessage("Le code doit être au format: AAAA-MM-JJ-001")
                .When(x => !string.IsNullOrEmpty(x.Code));
        }
    }
}