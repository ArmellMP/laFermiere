using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Ventes
{
    public class UpdateVenteRequest
    {
        [Required(ErrorMessage = "L'ID de la vente est requis")]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateVente { get; set; }

        [RegularExpression("^(PouletsVivants|PouletsNettoyes|PouletsFumes|Fientes|Autres)$", 
            ErrorMessage = "Type de vente invalide")]
        public string? TypeVente { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "La quantité doit être entre 0.01 et 1 000 000")]
        public decimal? Quantite { get; set; }

        [RegularExpression("^(Unite|Kg|Sac|Camion|Rouleau|Paquet)$", 
            ErrorMessage = "Unité de mesure invalide")]
        public string? UniteMesure { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Le prix unitaire doit être entre 0.01 et 1 000 000")]
        public decimal? PrixUnitaire { get; set; }

        [RegularExpression("^(Especes|PaiementMobile|VirementBancaire|Cheque|Credit)$", 
            ErrorMessage = "Mode de paiement invalide")]
        public string? ModePaiement { get; set; }

        public bool? EstPaye { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DatePaiement { get; set; }

        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent dépasser 2000 caractères")]
        public string? Observations { get; set; }
    }
}