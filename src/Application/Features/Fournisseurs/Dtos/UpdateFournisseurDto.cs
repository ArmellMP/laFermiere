namespace Application.Features.Fournisseurs.Dtos
{
    public class UpdateFournisseurDto
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? TypeFournisseur { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? ContactPersonne { get; set; }
        public string? Notes { get; set; }
        public bool? EstActif { get; set; }
    }
}