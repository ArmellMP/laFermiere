using System.Text.Json.Serialization;

namespace PresentationApi.Models.Responses
{
    public class AlerteResponse
    {
        public int Id { get; set; }
        
        [JsonPropertyName("typeAlerte")]
        public string TypeAlerte { get; set; } = string.Empty;
        
        [JsonPropertyName("typeAlerteLibelle")]
        public string TypeAlerteLibelle { get; set; } = string.Empty;
        
        [JsonPropertyName("entiteId")]
        public int EntiteId { get; set; }
        
        public string Niveau { get; set; } = string.Empty;
        
        [JsonPropertyName("niveauLibelle")]
        public string NiveauLibelle { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("estLue")]
        public bool EstLue { get; set; }
        
        [JsonPropertyName("dateLecture")]
        public DateTime? DateLecture { get; set; }
        
        [JsonPropertyName("dateAlerte")]
        public DateTime DateAlerte { get; set; }
        
        [JsonPropertyName("utilisateurId")]
        public int? UtilisateurId { get; set; }
        
        [JsonPropertyName("utilisateurNom")]
        public string? UtilisateurNom { get; set; }
    }
}