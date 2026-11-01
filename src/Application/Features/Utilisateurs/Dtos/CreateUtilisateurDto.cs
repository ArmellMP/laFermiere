namespace Application.Features.Utilisateurs.Dtos
{
    public class CreateUtilisateurDto
    {
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;  // Admin, Fermier, Veterinaire, Comptable
    }
}