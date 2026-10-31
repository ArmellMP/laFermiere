namespace Application.Features.Achats.Dtos
{
    public class CreateAchatDto
    {
        public int FournisseurId { get; set; }
        public int BandeId { get; set; }
        public DateTime DateAchat { get; set; }
        public string TypeAchat { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantite { get; set; }
        public string UniteMesure { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
        public string? FactureNumero { get; set; }
        public DateTime? DateReception { get; set; }
        public string? Observations { get; set; }
    }
}