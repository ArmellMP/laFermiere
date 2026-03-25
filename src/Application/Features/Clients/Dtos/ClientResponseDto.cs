using Domain.Enums;

namespace Application.Features.Clients.Dtos
{
    public class ClientResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string TypeClient { get; set; } = string.Empty;
        public string TypeClientLibelle { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? Notes { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Informations additionnelles
        public int NombreVentes { get; set; }  // Rempli via JOIN
        public decimal MontantTotalAchats { get; set; }  // Rempli via JOIN
    }
}