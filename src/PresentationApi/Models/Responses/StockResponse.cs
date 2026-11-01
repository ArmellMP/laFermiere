// PresentationApi/Models/Responses/StockResponse.cs
using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    /// <summary>
    /// Réponse API pour les stocks.
    /// </summary>
    public class StockResponse
    {
        public int Id { get; set; }
        public int BandeId { get; set; }
        
        [JsonPropertyName("bandeCode")]
        public string? BandeCode { get; set; }
        
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("typeLibelle")]
        public string TypeLibelle { get; set; } = string.Empty;
        
        public string Nom { get; set; } = string.Empty;
        
        [JsonPropertyName("quantiteInitiale")]
        public decimal QuantiteInitiale { get; set; }
        
        [JsonPropertyName("quantiteActuelle")]
        public decimal QuantiteActuelle { get; set; }
        
        [JsonPropertyName("uniteMesure")]
        public string UniteMesure { get; set; } = string.Empty;
        
        [JsonPropertyName("uniteMesureLibelle")]
        public string UniteMesureLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("seuilAlerte")]
        public decimal? SeuilAlerte { get; set; }
        
        [JsonPropertyName("estEnAlerte")]
        public bool EstEnAlerte { get; set; }
        
        [JsonPropertyName("datePeremption")]
        public DateTime? DatePeremption { get; set; }
        
        [JsonPropertyName("estPerime")]
        public bool EstPerime { get; set; }
        
        [JsonPropertyName("joursAvantPeremption")]
        public int JoursAvantPeremption { get; set; }
        
        [JsonPropertyName("prixUnitaire")]
        public decimal? PrixUnitaire { get; set; }
        
        [JsonPropertyName("valeurStock")]
        public decimal ValeurStock { get; set; }
        
        [JsonPropertyName("fournisseurId")]
        public int? FournisseurId { get; set; }
        
        [JsonPropertyName("fournisseurNom")]
        public string? FournisseurNom { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}