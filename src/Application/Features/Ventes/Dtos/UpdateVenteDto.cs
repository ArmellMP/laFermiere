namespace Application.Features.Ventes.Dtos
{
    public class UpdateVenteDto
    {
        public int Id { get; set; }
        public DateTime? DateVente { get; set; }
        public string? TypeVente { get; set; }
        public decimal? Quantite { get; set; }
        public string? UniteMesure { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public string? ModePaiement { get; set; }
        public bool? EstPaye { get; set; }
        public DateTime? DatePaiement { get; set; }
        public string? Observations { get; set; }
    }
}