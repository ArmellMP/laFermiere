namespace Application.Features.Utilisateurs.Dtos
{
    public class UtilisateurResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RoleLibelle { get; set; } = string.Empty;
        public bool EstActif { get; set; }
        public DateTime? DerniereConnexion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Propriétés calculées
        public bool EstAdmin => Role == "Admin";
        public bool EstFermier => Role == "Fermier";
        public bool EstVeterinaire => Role == "Veterinaire";
        public bool EstComptable => Role == "Comptable";
    }
}