namespace Application.Features.Fournisseurs.Dtos
{
    public class FournisseurResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string TypeFournisseur { get; set; } = string.Empty;
        public string TypeFournisseurLibelle { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? ContactPersonne { get; set; }
        public string? Notes { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Informations additionnelles
        public int NombreAchats { get; set; }  // Rempli via JOIN
        public decimal MontantTotalAchats { get; set; }  // Rempli via JOIN
    }
}