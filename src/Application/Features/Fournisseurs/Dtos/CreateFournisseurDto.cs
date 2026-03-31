namespace Application.Features.Fournisseurs.Dtos
{
    public class CreateFournisseurDto
    {
        public string Nom { get; set; } = string.Empty;
        public string TypeFournisseur { get; set; } = string.Empty;  // "Poussiniere", "Aliment", etc.
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? ContactPersonne { get; set; }
        public string? Notes { get; set; }
    }
}