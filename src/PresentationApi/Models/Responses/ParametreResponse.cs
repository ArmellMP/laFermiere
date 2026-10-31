using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class ParametreResponse
    {
        public int Id { get; set; }
        public string Cle { get; set; } = string.Empty;
        public string? Valeur { get; set; }
        public string? Description { get; set; }
        public string Categorie { get; set; } = string.Empty;
        
        [JsonPropertyName("categorieLibelle")]
        public string CategorieLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("estModifiable")]
        public bool EstModifiable { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}