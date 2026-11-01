using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Alertes
{
    public class CreateAlerteRequest
    {
        [Required(ErrorMessage = "Le type d'alerte est requis")]
        [RegularExpression("^(StockFaible|Peremption|MortaliteElevee|Temperature|PeseeManquante|BandeACloturer)$", 
            ErrorMessage = "Type d'alerte invalide")]
        public string TypeAlerte { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'ID de l'entité est requis")]
        public int EntiteId { get; set; }

        [Required(ErrorMessage = "Le niveau d'alerte est requis")]
        [RegularExpression("^(Info|Avertissement|Critique)$", 
            ErrorMessage = "Niveau d'alerte invalide")]
        public string Niveau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le message est requis")]
        [MaxLength(65535, ErrorMessage = "Le message est trop long")]
        public string Message { get; set; } = string.Empty;
    }
}