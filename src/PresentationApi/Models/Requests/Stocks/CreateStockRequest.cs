using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Stocks
{
    /// <summary>
    /// Requête pour créer un nouveau stock.
    /// </summary>
    public class CreateStockRequest
    {
        [Required(ErrorMessage = "L'ID de la bande est requis")]
        public int BandeId { get; set; }

        [Required(ErrorMessage = "Le type de stock est requis")]
        [RegularExpression("^(Aliment|Medicament|Vaccin|Materiel|Autre)$", 
            ErrorMessage = "Type invalide. Valeurs acceptées : Aliment, Medicament, Vaccin, Materiel, Autre")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du produit est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "La quantité initiale est requise")]
        [Range(0.01, 1000000, ErrorMessage = "La quantité doit être entre 0.01 et 1 000 000")]
        public decimal QuantiteInitiale { get; set; }

        [Required(ErrorMessage = "L'unité de mesure est requise")]
        [RegularExpression("^(Kg|g|L|ml|Unite|Sac|Dose|Rouleau|Paquet)$", 
            ErrorMessage = "Unité invalide. Valeurs acceptées : Kg, g, L, ml, Unite, Sac, Dose, Rouleau, Paquet")]
        public string UniteMesure { get; set; } = string.Empty;

        [Range(0, 1000000, ErrorMessage = "Le seuil d'alerte doit être positif")]
        public decimal? SeuilAlerte { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DatePeremption { get; set; }

        [Range(0, 1000000, ErrorMessage = "Le prix unitaire doit être positif")]
        public decimal? PrixUnitaire { get; set; }

        public int? FournisseurId { get; set; }
    }
}