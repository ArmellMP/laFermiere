// Application/Features/Stocks/Validators/UpdateStockDtoValidator.cs
using FluentValidation;
using Application.Features.Stocks.Dtos;

namespace Application.Features.Stocks.Validators
{
    /// <summary>
    /// Validateur pour le DTO de mise à jour d'un stock.
    /// Note: En mise à jour, on ne peut modifier que certaines propriétés.
    /// </summary>
    public class UpdateStockDtoValidator : AbstractValidator<UpdateStockDto>
    {
        public UpdateStockDtoValidator()
        {
            
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("L'ID du stock est requis pour la mise à jour.");
            // En mise à jour, on DOIT savoir quel stock modifier
            
            RuleFor(x => x.QuantiteActuelle)
                .GreaterThanOrEqualTo(0)  // Peut être 0 (stock épuisé)
                .WithMessage("La quantité actuelle ne peut pas être négative.")
                .LessThanOrEqualTo(1000000)
                .WithMessage("La quantité actuelle ne peut pas dépasser 1 000 000.")
                .PrecisionScale(10, 2, true)
                .WithMessage("La quantité ne peut pas avoir plus de 2 décimales.");
            // Note: On ne peut PAS modifier QuantiteInitiale en mise à jour

            // ============================================
            // RÈGLE 3 : Validation conditionnelle du SeuilAlerte
            // ============================================
            
            When(x => x.SeuilAlerte.HasValue, () =>
            {
                RuleFor(x => x.SeuilAlerte)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Le seuil d'alerte ne peut pas être négatif.");
                // Note: En mise à jour, on ne peut pas comparer avec QuantiteInitiale
                // car elle n'est pas dans le DTO de mise à jour
            });

            // ============================================
            // RÈGLE 4 : Validation conditionnelle de DatePeremption
            // ============================================
            
            When(x => x.DatePeremption.HasValue, () =>
            {
                // Pas d'obligation "dans le futur" en modification (décision 2026-11-01) :
                // il faut pouvoir corriger ou compléter un stock déjà périmé.
                // (À la création, CreateStockDtoValidator exige toujours une date future.)
                RuleFor(x => x.DatePeremption)
                    .LessThan(DateTime.UtcNow.AddYears(5))
                    .WithMessage("La date de péremption est trop éloignée (max 5 ans).");
            });

            // ============================================
            // RÈGLE 5 : Validation conditionnelle de PrixUnitaire
            // ============================================
            
            When(x => x.PrixUnitaire.HasValue, () =>
            {
                RuleFor(x => x.PrixUnitaire)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Le prix unitaire ne peut pas être négatif.")
                    .PrecisionScale(12, 2, true)
                    .WithMessage("Le prix ne peut pas avoir plus de 2 décimales.");
            });

            // ============================================
            // RÈGLE 6 : Validation d'intégrité (règles globales)
            // ============================================
            
            // Exemple: Si le stock est périmé, on ne devrait pas pouvoir augmenter la quantité
            RuleFor(x => x)
                .Must(NotIncreaseQuantityIfExpired)
                .WithMessage("Impossible d'augmenter la quantité d'un stock périmé.")
                .When(x => x.QuantiteActuelle > 0 && x.DatePeremption < DateTime.UtcNow);
            // Note: Must prend une fonction qui reçoit l'OBJET ENTIER (x => x)
        }

        /// <summary>
        /// Vérifie qu'on n'augmente pas la quantité d'un stock périmé
        /// </summary>
        private bool NotIncreaseQuantityIfExpired(UpdateStockDto dto)
        {
            // Cette méthode serait appelée avec le stock EXISTANT en base
            // Pour l'instant, c'est un exemple de règle complexe
            return true; // À implémenter avec le repository
        }
    }
}