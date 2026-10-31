using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class AchatResponse
    {
        public int Id { get; set; }
        public int FournisseurId { get; set; }
        public string? FournisseurNom { get; set; }
        public int BandeId { get; set; }
        public string? BandeCode { get; set; }
        
        [JsonPropertyName("dateAchat")]
        public DateTime DateAchat { get; set; }
        
        [JsonPropertyName("typeAchat")]
        public string TypeAchat { get; set; } = string.Empty;
        
        [JsonPropertyName("typeAchatLibelle")]
        public string TypeAchatLibelle { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public decimal Quantite { get; set; }
        
        [JsonPropertyName("uniteMesure")]
        public string UniteMesure { get; set; } = string.Empty;
        
        [JsonPropertyName("uniteMesureLibelle")]
        public string UniteMesureLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("prixUnitaire")]
        public decimal PrixUnitaire { get; set; }
        
        [JsonPropertyName("montantTotal")]
        public decimal MontantTotal { get; set; }
        
        [JsonPropertyName("factureNumero")]
        public string? FactureNumero { get; set; }
        
        [JsonPropertyName("dateReception")]
        public DateTime? DateReception { get; set; }
        
        [JsonPropertyName("estRecu")]
        public bool EstRecu { get; set; }
        
        [JsonPropertyName("joursEntreAchatEtReception")]
        public int JoursEntreAchatEtReception { get; set; }
        
        public string? Observations { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("createdBy")]
        public int? CreatedBy { get; set; }
        
        [JsonPropertyName("createurNom")]
        public string? CreateurNom { get; set; }
    }
}