// PresentationApi/Models/Responses/BandeResponse.cs
using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class BandeResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        
        [JsonPropertyName("dateArrivee")]
        public DateTime DateArrivee { get; set; }
        
        [JsonPropertyName("nbPoussinsInit")]
        public int NbPoussinsInit { get; set; }
        
        [JsonPropertyName("nbPoussinsActuel")]
        public int NbPoussinsActuel { get; set; }
        
        public string Statut { get; set; } = string.Empty;
        
        [JsonPropertyName("statutLibelle")]
        public string StatutLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("dateCloture")]
        public DateTime? DateCloture { get; set; }
        
        public string? Observations { get; set; }
        
        [JsonPropertyName("superficieCalculee")]
        public decimal SuperficieCalculee { get; set; }
        
        [JsonPropertyName("tauxMortalite")]
        public decimal TauxMortalite { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        // Propriétés optionnelles pour les détails
        [JsonPropertyName("totalMortalites")]
        public int? TotalMortalites { get; set; }
        
        [JsonPropertyName("joursEcoules")]
        public int? JoursEcoules { get; set; }
        
        [JsonPropertyName("joursRestants")]
        public int? JoursRestants { get; set; }
        
        [JsonPropertyName("pourcentageAvancement")]
        public decimal? PourcentageAvancement { get; set; }
    }
}