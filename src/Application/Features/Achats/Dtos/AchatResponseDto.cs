namespace Application.Features.Achats.Dtos
{
    public class AchatResponseDto
    {
        public int Id { get; set; }
        public int FournisseurId { get; set; }
        public string? FournisseurNom { get; set; }
        public int BandeId { get; set; }
        public string? BandeCode { get; set; }
        public DateTime DateAchat { get; set; }
        public string TypeAchat { get; set; } = string.Empty;
        public string TypeAchatLibelle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantite { get; set; }
        public string UniteMesure { get; set; } = string.Empty;
        public string UniteMesureLibelle { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        public string? FactureNumero { get; set; }
        public DateTime? DateReception { get; set; }
        public bool EstRecu { get; set; }
        public int JoursEntreAchatEtReception { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreateurNom { get; set; }
    }
}