// PresentationApi/Models/Responses/VenteResponse.cs
using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class VenteResponse
    {
        public int Id { get; set; }
        public int BandeId { get; set; }
        public string? BandeCode { get; set; }
        public int ClientId { get; set; }
        public string? ClientNom { get; set; }
        
        [JsonPropertyName("dateVente")]
        public DateTime DateVente { get; set; }
        
        [JsonPropertyName("typeVente")]
        public string TypeVente { get; set; } = string.Empty;
        
        [JsonPropertyName("typeVenteLibelle")]
        public string TypeVenteLibelle { get; set; } = string.Empty;
        
        public decimal Quantite { get; set; }
        
        [JsonPropertyName("uniteMesure")]
        public string UniteMesure { get; set; } = string.Empty;
        
        [JsonPropertyName("uniteMesureLibelle")]
        public string UniteMesureLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("prixUnitaire")]
        public decimal PrixUnitaire { get; set; }
        
        [JsonPropertyName("montantTotal")]
        public decimal MontantTotal { get; set; }
        
        [JsonPropertyName("modePaiement")]
        public string ModePaiement { get; set; } = string.Empty;
        
        [JsonPropertyName("modePaiementLibelle")]
        public string ModePaiementLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("estPaye")]
        public bool EstPaye { get; set; }
        
        [JsonPropertyName("datePaiement")]
        public DateTime? DatePaiement { get; set; }
        
        public string? Observations { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("createdBy")]
        public int? CreatedBy { get; set; }
        
        [JsonPropertyName("createurNom")]
        public string? CreateurNom { get; set; }
    }
}