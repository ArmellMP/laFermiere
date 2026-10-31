using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    /// <summary>
    /// Réponse API pour les clients.
    /// </summary>
    public class ClientResponse
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        
        [JsonPropertyName("typeClient")]
        public string TypeClient { get; set; } = string.Empty;
        
        [JsonPropertyName("typeClientLibelle")]
        public string TypeClientLibelle { get; set; } = string.Empty;
        
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? Notes { get; set; }
        
        [JsonPropertyName("estActif")]
        public bool EstActif { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("nombreVentes")]
        public int? NombreVentes { get; set; }
        
        [JsonPropertyName("montantTotalAchats")]
        public decimal? MontantTotalAchats { get; set; }
    }
}