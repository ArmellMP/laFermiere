using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class UtilisateurResponse
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
        [JsonPropertyName("roleLibelle")]
        public string RoleLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("estActif")]
        public bool EstActif { get; set; }
        
        [JsonPropertyName("derniereConnexion")]
        public DateTime? DerniereConnexion { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("estAdmin")]
        public bool EstAdmin { get; set; }
        
        [JsonPropertyName("estFermier")]
        public bool EstFermier { get; set; }
        
        [JsonPropertyName("estVeterinaire")]
        public bool EstVeterinaire { get; set; }
        
        [JsonPropertyName("estComptable")]
        public bool EstComptable { get; set; }
    }
}