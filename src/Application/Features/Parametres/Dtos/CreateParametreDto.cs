namespace Application.Features.Parametres.Dtos
{
    public class CreateParametreDto
    {
        public string Cle { get; set; } = string.Empty;
        public string? Valeur { get; set; }
        public string? Description { get; set; }
        public string Categorie { get; set; } = string.Empty;  // "Elevage", "Stock", etc.
        public bool EstModifiable { get; set; } = true;
    }
}