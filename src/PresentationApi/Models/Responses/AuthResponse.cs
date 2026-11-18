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

        /// <summary>À envoyer à /refresh quand le jeton expire, et à /logout pour se déconnecter.</summary>
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("refreshExpiration")]
        public DateTime? RefreshExpiration { get; set; }
    }
}