using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        
        [JsonPropertyName("utilisateur")]
        public UtilisateurResponse? Utilisateur { get; set; }
        
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        
        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }
    }
}