namespace Application.Features.Utilisateurs.Dtos
{
    public class UpdateUtilisateurDto
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool? EstActif { get; set; }
    }
}