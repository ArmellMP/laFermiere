// PresentationApi/Models/Requests/ConsommerStockRequest.cs
using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Stocks
{
    /// <summary>
    /// Requête pour consommer une quantité d'un stock.
    /// </summary>
    public class ConsommerStockRequest
    {
        [Required(ErrorMessage = "L'ID du stock est requis")]
        public int StockId { get; set; }

        [Required(ErrorMessage = "La quantité consommée est requise")]
        [Range(0.01, 1000000, ErrorMessage = "La quantité doit être entre 0.01 et 1 000 000")]
        public decimal QuantiteConsommee { get; set; }

        public int? SuiviJournalierId { get; set; }
    }
}