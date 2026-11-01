using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class FournisseurResponse
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        
        [JsonPropertyName("typeFournisseur")]
        public string TypeFournisseur { get; set; } = string.Empty;
        
        [JsonPropertyName("typeFournisseurLibelle")]
        public string TypeFournisseurLibelle { get; set; } = string.Empty;
        
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        
        [JsonPropertyName("contactPersonne")]
        public string? ContactPersonne { get; set; }
        
        public string? Notes { get; set; }
        
        [JsonPropertyName("estActif")]
        public bool EstActif { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("nombreAchats")]
        public int? NombreAchats { get; set; }
        
        [JsonPropertyName("montantTotalAchats")]
        public decimal? MontantTotalAchats { get; set; }
    }
}