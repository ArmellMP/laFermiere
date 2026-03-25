namespace Application.Features.SuivisJournaliers.Dtos
{
    /// <summary>
    /// DTO pour la création d'un suivi journalier. 
    /// </summary>
    public class CreateSuiviDto
    {
        public int BandeId { get; set; }           
        public int Jour { get; set; }               // Jour du cycle (Entre 1 et 45)
        public DateTime DateSuivi { get; set; }     // Date du suivi
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
        public decimal? Temperature { get; set; }      // Température dans le bâtiment
        public string? Observations { get; set; }      
    }
}