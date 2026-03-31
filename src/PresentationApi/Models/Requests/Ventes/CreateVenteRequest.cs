using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Ventes
{
    public class CreateVenteRequest
    {
        [Required(ErrorMessage = "L'ID de la bande est requis")]
        public int BandeId { get; set; }

        [Required(ErrorMessage = "L'ID du client est requis")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "La date de vente est requise")]
        [DataType(DataType.Date)]
        public DateTime DateVente { get; set; }

        [Required(ErrorMessage = "Le type de vente est requis")]
        [RegularExpression("^(PouletsVivants|PouletsNettoyes|PouletsFumes|Fientes|Autres)$", 
            ErrorMessage = "Type de vente invalide")]
        public string TypeVente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La quantité est requise")]
        [Range(0.01, 1000000, ErrorMessage = "La quantité doit être entre 0.01 et 1 000 000")]
        public decimal Quantite { get; set; }

        [Required(ErrorMessage = "L'unité de mesure est requise")]
        [RegularExpression("^(Unite|Kg|Sac|Camion|Rouleau|Paquet)$", 
            ErrorMessage = "Unité de mesure invalide")]
        public string UniteMesure { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prix unitaire est requis")]
        [Range(0.01, 1000000, ErrorMessage = "Le prix unitaire doit être entre 0.01 et 1 000 000")]
        public decimal PrixUnitaire { get; set; }

        [Required(ErrorMessage = "Le mode de paiement est requis")]
        [RegularExpression("^(Especes|PaiementMobile|VirementBancaire|Cheque|Credit)$", 
            ErrorMessage = "Mode de paiement invalide")]
        public string ModePaiement { get; set; } = string.Empty;

        public bool EstPaye { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DatePaiement { get; set; }

        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent dépasser 2000 caractères")]
        public string? Observations { get; set; }
    }
}