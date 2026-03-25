namespace Application.Features.SuivisJournaliers.Dtos
{
    /// <summary>
    /// DTO pour la mise à jour d'un suivi journalier.
    /// </summary>
    public class UpdateSuiviDto
    {
        public int Id { get; set; }                     // L'ID du suivi à modifier
        public int? NbMortalites { get; set; }
        public string? CauseMortalite { get; set; }
        public decimal? PoidsMoyen { get; set; }
        public int? NbSujetsPeses { get; set; }
        public string? TypeSoin { get; set; }
        public string? ProduitUtilise { get; set; }
        public string? Dosage { get; set; }
        public int? DureeTraitement { get; set; }
        public decimal? CoutSoin { get; set; }
        public int? StockConsommeId { get; set; }
        public decimal? QuantiteConsommee { get; set; }
        public decimal? Temperature { get; set; }
        public string? Observations { get; set; }
        
    }
}