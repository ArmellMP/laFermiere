using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Parametres
{
    public class UpdateParametreRequest
    {
        [Required(ErrorMessage = "L'ID du paramètre est requis")]
        public int Id { get; set; }

        public string? Valeur { get; set; }

        [MaxLength(200, ErrorMessage = "La description ne peut dépasser 200 caractères")]
        public string? Description { get; set; }

        [RegularExpression("^(Elevage|Stock|Finance|Systeme|General)$", 
            ErrorMessage = "Catégorie invalide")]
        public string? Categorie { get; set; }

        public bool? EstModifiable { get; set; }
    }
}