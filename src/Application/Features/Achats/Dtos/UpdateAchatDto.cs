namespace Application.Features.Achats.Dtos
{
    public class UpdateAchatDto
    {
        public int Id { get; set; }
        public DateTime? DateAchat { get; set; }
        public string? TypeAchat { get; set; }
        public string? Description { get; set; }
        public decimal? Quantite { get; set; }
        public string? UniteMesure { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public string? FactureNumero { get; set; }
        public DateTime? DateReception { get; set; }
        public string? Observations { get; set; }
    }
}