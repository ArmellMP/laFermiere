namespace Application.Features.Clients.Dtos
{
    public class CreateClientDto
    {
        public string Nom { get; set; } = string.Empty;
        public string TypeClient { get; set; } = string.Empty;  // "Particulier", "Restaurant", etc.
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? Notes { get; set; }
    }
}