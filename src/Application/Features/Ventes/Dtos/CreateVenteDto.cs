namespace Application.Features.Ventes.Dtos
{
    public class CreateVenteDto
    {
        public int BandeId { get; set; }
        public int ClientId { get; set; }
        public DateTime DateVente { get; set; }
        public string TypeVente { get; set; } = string.Empty;
        public decimal Quantite { get; set; }
        public string UniteMesure { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
        public string ModePaiement { get; set; } = string.Empty;
        public bool EstPaye { get; set; }
        public DateTime? DatePaiement { get; set; }
        public string? Observations { get; set; }
    }
}