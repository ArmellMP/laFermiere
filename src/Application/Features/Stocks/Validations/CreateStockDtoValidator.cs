// Application/Features/Stocks/Validators/CreateStockDtoValidator.cs
using FluentValidation;
using Application.Features.Stocks.Dtos;
using Domain.Enums;

namespace Application.Features.Stocks.Validators
{
    /// <summary>
    /// Validateur pour le DTO de création d'un stock.
    /// Vérifie que toutes les données envoyées par le client sont valides
    /// AVANT d'atteindre le Handler.
    /// </summary>
    public class CreateStockDtoValidator : AbstractValidator<CreateStockDto>
    {
        public CreateStockDtoValidator()
        {
            
            RuleFor(x => x.BandeId)
                .GreaterThan(0)  // BandeId doit être > 0 (les ID en BDD commencent à 1)
                .WithMessage("L'ID de la bande doit être un nombre positif.");
            // Explication : 
            // - RuleFor : on définit une règle pour une propriété
            // - GreaterThan(0) : la valeur doit être > 0
            // - WithMessage : message d'erreur personnalisé
            
            RuleFor(x => x.Type)
                .NotEmpty()  // Ne peut pas être vide
                .WithMessage("Le type de stock est requis.")
                .Must(BeValidType)  // Vérification personnalisée
                .WithMessage("Type de stock invalide. Valeurs acceptées : Aliment, Medicament, Vaccin, Materiel, Autre.");
            // Must(BeValidType) : appelle une méthode privée qui retourne true/false
            
            RuleFor(x => x.Nom)
                .NotEmpty()  // Champ obligatoire
                .WithMessage("Le nom du produit est requis.")
                .MaximumLength(100)  // Longueur maximale (comme dans la BDD VARCHAR(100))
                .WithMessage("Le nom du produit ne peut pas dépasser 100 caractères.")
                .Matches(@"^[a-zA-Z0-9\s\-éèêëàâäôöùûüç]+$")  // Expression régulière
                .WithMessage("Le nom contient des caractères non autorisés.");
            // Matches : vérifie que le texte correspond à un pattern (lettres, chiffres, espaces, tirets, accents)
            
            RuleFor(x => x.QuantiteInitiale)
                .GreaterThan(0)  // Strictement positif
                .WithMessage("La quantité initiale doit être supérieure à 0.")
                .LessThanOrEqualTo(1000000)  // Maximum 1 million (valeur arbitraire)
                .WithMessage("La quantité initiale ne peut pas dépasser 1 000 000.")
                .PrecisionScale(10, 2, true)  // 10 chiffres max, 2 décimales
                .WithMessage("La quantité ne peut pas avoir plus de 2 décimales.");
            // PrecisionScale : pour les nombres décimaux (comme DECIMAL(10,2) en SQL)
            
            RuleFor(x => x.UniteMesure)
                .NotEmpty()
                .WithMessage("L'unité de mesure est requise.")
                .Must(BeValidUniteMesure)
                .WithMessage("Unité de mesure invalide. Valeurs acceptées : Kg, g, L, ml, Unite, Sac, Dose, Rouleau, Paquet.");

            // Quand la propriété SeuilAlerte est fournie (pas null)
            When(x => x.SeuilAlerte.HasValue, () =>
            {
                RuleFor(x => x.SeuilAlerte)
                    .GreaterThanOrEqualTo(0)  // Ne peut pas être négatif
                    .WithMessage("Le seuil d'alerte ne peut pas être négatif.")
                    .LessThanOrEqualTo(x => x.QuantiteInitiale)  // Compare avec QuantiteInitiale
                    .WithMessage("Le seuil d'alerte ne peut pas dépasser la quantité initiale.");
                // LessThanOrEqualTo(x => x.QuantiteInitiale) : compare avec une AUTRE propriété du DTO
            });

            // ============================================
            // RÈGLE 7 : Validation conditionnelle de DatePeremption
            // ============================================
            
            When(x => x.DatePeremption.HasValue, () =>
            {
                RuleFor(x => x.DatePeremption)
                    .GreaterThan(DateTime.UtcNow)  // Doit être dans le futur
                    .WithMessage("La date de péremption doit être dans le futur.")
                    .LessThan(DateTime.UtcNow.AddYears(5))  // Maximum 5 ans dans le futur
                    .WithMessage("La date de péremption est trop éloignée (max 5 ans).");
            });

            // ============================================
            // RÈGLE 8 : Validation conditionnelle de PrixUnitaire
            // ============================================
            
            When(x => x.PrixUnitaire.HasValue, () =>
            {
                RuleFor(x => x.PrixUnitaire)
                    .GreaterThanOrEqualTo(0)  // Prix peut être 0 (gratuit) mais pas négatif
                    .WithMessage("Le prix unitaire ne peut pas être négatif.")
                    .PrecisionScale(12, 2, true)  // DECIMAL(12,2) comme en SQL
                    .WithMessage("Le prix ne peut pas avoir plus de 2 décimales.");
            });

            // ============================================
            // RÈGLE 9 : Validation conditionnelle de FournisseurId
            // ============================================
            
            When(x => x.FournisseurId.HasValue, () =>
            {
                RuleFor(x => x.FournisseurId)
                    .GreaterThan(0)  // ID valide
                    .WithMessage("L'ID du fournisseur doit être un nombre positif.");
            });
        }

        // ============================================
        // MÉTHODES PRIVÉES DE VALIDATION PERSONNALISÉE
        // ============================================

        /// <summary>
        /// Vérifie si le type de stock est valide (existe dans l'enum TypeStock)
        /// </summary>
        private bool BeValidType(string type)
        {
            // Enum.GetNames<TypeStock>() retourne un tableau des noms de l'enum
            // Exemple: ["Aliment", "Medicament", "Vaccin", "Materiel", "Autre"]
            // Contains vérifie si la valeur fournie est dans ce tableau
            return Enum.GetNames<TypeStock>().Contains(type);
        }

        /// <summary>
        /// Vérifie si l'unité de mesure est valide (existe dans l'enum UniteMesure)
        /// </summary>
        /// <param name="unite">L'unité à vérifier (string)</param>
        /// <returns>true si l'unité existe dans l'enum</returns>
        private bool BeValidUniteMesure(string unite)
        {
            return Enum.GetNames<UniteMesure>().Contains(unite);
        }

        /// <summary>
        /// Exemple de validation personnalisée plus complexe
        /// Vérifie que le format du code est correct
        /// </summary>
        private bool BeValidCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return true; // Code optionnel
                
            // Format: Année-Mois-Jour (ex: 2024-03-12)
            return System.Text.RegularExpressions.Regex.IsMatch(code, 
                @"^\d{4}-\d{2}-\d{2}-\d{3}$");
        }
    }
}