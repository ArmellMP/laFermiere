using System;

namespace Application.Features.Stocks.Dtos
{
    public class UpdateStockDto
    {
        public int Id { get; set; }
        public decimal QuantiteActuelle { get; set; }
        public decimal? SeuilAlerte { get; set; }
        public DateTime? DatePeremption { get; set; }
        public decimal? PrixUnitaire { get; set; }
        // Note : Type, Nom, UniteMesure ne sont pas modifiables après création
    }
}
