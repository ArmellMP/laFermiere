namespace Application.Features.Parametres.Dtos
{
    public class ParametreResponseDto
    {
        public int Id { get; set; }
        public string Cle { get; set; } = string.Empty;
        public string? Valeur { get; set; }
        public string? Description { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public string CategorieLibelle { get; set; } = string.Empty;
        public bool EstModifiable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}