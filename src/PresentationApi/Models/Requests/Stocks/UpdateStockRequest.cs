using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Stocks
{
    /// <summary>
    /// Requête pour mettre à jour un stock existant.
    /// </summary>
    public class UpdateStockRequest
    {
        [Required(ErrorMessage = "L'ID du stock est requis")]
        public int Id { get; set; }

        [Required(ErrorMessage = "La quantité actuelle est requise")]
        [Range(0, 1000000, ErrorMessage = "La quantité doit être entre 0 et 1 000 000")]
        public decimal QuantiteActuelle { get; set; }

        [Range(0, 1000000, ErrorMessage = "Le seuil d'alerte doit être positif")]
        public decimal? SeuilAlerte { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DatePeremption { get; set; }

        [Range(0, 1000000, ErrorMessage = "Le prix unitaire doit être positif")]
        public decimal? PrixUnitaire { get; set; }
    }
}