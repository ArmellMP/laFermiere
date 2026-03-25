using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Suivis
{
    public class UpdateSuiviRequest
    {
        [Required(ErrorMessage = "L'ID du suivi est requis")]
        public int Id { get; set; }

        [Range(0, 1000, ErrorMessage = "Le nombre de mortalités doit être entre 0 et 1000")]
        public int? NbMortalites { get; set; }

        [MaxLength(200, ErrorMessage = "La cause de mortalité ne peut dépasser 200 caractères")]
        public string? CauseMortalite { get; set; }

        [Range(0.01, 10, ErrorMessage = "Le poids moyen doit être entre 0.01 et 10 kg")]
        public decimal? PoidsMoyen { get; set; }

        [Range(1, 1000, ErrorMessage = "Le nombre de sujets pesés doit être entre 1 et 1000")]
        public int? NbSujetsPeses { get; set; }

        [RegularExpression("^(Vaccination|Traitement|Prevention|Supplement|Autre)$", 
            ErrorMessage = "Type de soin invalide")]
        public string? TypeSoin { get; set; }

        [MaxLength(100, ErrorMessage = "Le produit utilisé ne peut dépasser 100 caractères")]
        public string? ProduitUtilise { get; set; }

        [MaxLength(100, ErrorMessage = "Le dosage ne peut dépasser 100 caractères")]
        public string? Dosage { get; set; }

        [Range(1, 30, ErrorMessage = "La durée du traitement doit être entre 1 et 30 jours")]
        public int? DureeTraitement { get; set; }

        [Range(0, 1000000, ErrorMessage = "Le coût du soin doit être entre 0 et 1 000 000")]
        public decimal? CoutSoin { get; set; }

        public int? StockConsommeId { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "La quantité consommée doit être entre 0.01 et 1 000 000")]
        public decimal? QuantiteConsommee { get; set; }

        [Range(-10, 50, ErrorMessage = "La température doit être comprise entre -10 et 50°C")]
        public decimal? Temperature { get; set; }

        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent dépasser 2000 caractères")]
        public string? Observations { get; set; }
    }
}