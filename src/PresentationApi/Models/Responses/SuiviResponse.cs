using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class SuiviResponse
    {
        public int Id { get; set; }
        public int BandeId { get; set; }
        public string? BandeCode { get; set; }
        public int Jour { get; set; }
        
        [JsonPropertyName("dateSuivi")]
        public DateTime DateSuivi { get; set; }
        
        [JsonPropertyName("nbMortalites")]
        public int NbMortalites { get; set; }
        
        [JsonPropertyName("causeMortalite")]
        public string? CauseMortalite { get; set; }
        
        [JsonPropertyName("poidsMoyen")]
        public decimal? PoidsMoyen { get; set; }
        
        [JsonPropertyName("nbSujetsPeses")]
        public int? NbSujetsPeses { get; set; }
        
        [JsonPropertyName("peseeEffectuee")]
        public bool PeseeEffectuee { get; set; }
        
        [JsonPropertyName("typeSoin")]
        public string? TypeSoin { get; set; }
        
        [JsonPropertyName("typeSoinLibelle")]
        public string? TypeSoinLibelle { get; set; }
        
        [JsonPropertyName("produitUtilise")]
        public string? ProduitUtilise { get; set; }
        
        public string? Dosage { get; set; }
        
        [JsonPropertyName("dureeTraitement")]
        public int? DureeTraitement { get; set; }
        
        [JsonPropertyName("coutSoin")]
        public decimal? CoutSoin { get; set; }
        
        [JsonPropertyName("stockConsommeId")]
        public int? StockConsommeId { get; set; }
        
        [JsonPropertyName("stockConsommeNom")]
        public string? StockConsommeNom { get; set; }
        
        [JsonPropertyName("quantiteConsommee")]
        public decimal? QuantiteConsommee { get; set; }
        
        [JsonPropertyName("uniteMesureStock")]
        public string? UniteMesureStock { get; set; }
        
        public decimal? Temperature { get; set; }
        
        [JsonPropertyName("temperatureAvis")]
        public string? TemperatureAvis { get; set; }
        
        public string? Observations { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("createdBy")]
        public int? CreatedBy { get; set; }
        
        [JsonPropertyName("createurNom")]
        public string? CreateurNom { get; set; }
        
        [JsonPropertyName("ageEnJours")]
        public int AgeEnJours { get; set; }
    }
}