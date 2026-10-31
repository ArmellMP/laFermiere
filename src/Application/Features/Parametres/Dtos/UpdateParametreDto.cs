namespace Application.Features.Parametres.Dtos
{
    public class UpdateParametreDto
    {
        public int Id { get; set; }
        public string? Valeur { get; set; }
        public string? Description { get; set; }
        public string? Categorie { get; set; }
        public bool? EstModifiable { get; set; }
    }
}