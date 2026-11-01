using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Parametres
{
    public class CreateParametreRequest
    {
        [Required(ErrorMessage = "La clé du paramètre est requise")]
        [MaxLength(50, ErrorMessage = "La clé ne peut dépasser 50 caractères")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "La clé ne peut contenir que des lettres, chiffres et tirets bas")]
        public string Cle { get; set; } = string.Empty;

        [MaxLength(65535, ErrorMessage = "La valeur est trop longue")]
        public string? Valeur { get; set; }

        [MaxLength(200, ErrorMessage = "La description ne peut dépasser 200 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La catégorie est requise")]
        [RegularExpression("^(Elevage|Stock|Finance|Systeme|General)$", 
            ErrorMessage = "Catégorie invalide. Valeurs acceptées : Elevage, Stock, Finance, Systeme, General")]
        public string Categorie { get; set; } = string.Empty;

        public bool EstModifiable { get; set; } = true;
    }
}