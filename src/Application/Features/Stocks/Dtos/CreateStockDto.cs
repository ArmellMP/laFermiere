namespace Application.Features.Stocks.Dtos
{
    public class CreateStockDto
    {
        public int BandeId { get; set; }
        public string Type { get; set; } = string.Empty;  // "Aliment", "Medicament", etc.
        public string Nom { get; set; } = string.Empty;
        public decimal QuantiteInitiale { get; set; }
        public string UniteMesure { get; set; } = string.Empty;  // "Kg", "L", etc.
        public decimal? SeuilAlerte { get; set; }
        public DateTime? DatePeremption { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public int? FournisseurId { get; set; }
    }
}