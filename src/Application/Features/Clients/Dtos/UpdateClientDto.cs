using Domain.Enums;

namespace Application.Features.Clients.Dtos
{
    public class UpdateClientDto
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? TypeClient { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? Notes { get; set; }
        public bool? EstActif { get; set; }
    }
}