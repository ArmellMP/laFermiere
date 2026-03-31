namespace Application.Features.Ventes.Dtos
{
    public class VenteResponseDto
    {
        public int Id { get; set; }
        public int BandeId { get; set; }
        public string? BandeCode { get; set; }
        public int ClientId { get; set; }
        public string? ClientNom { get; set; }
        public DateTime DateVente { get; set; }
        public string TypeVente { get; set; } = string.Empty;
        public string TypeVenteLibelle { get; set; } = string.Empty;
        public decimal Quantite { get; set; }
        public string UniteMesure { get; set; } = string.Empty;
        public string UniteMesureLibelle { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        public string ModePaiement { get; set; } = string.Empty;
        public string ModePaiementLibelle { get; set; } = string.Empty;
        public bool EstPaye { get; set; }
        public DateTime? DatePaiement { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreateurNom { get; set; }
    }
}