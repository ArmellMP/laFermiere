using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Achats
{
    public class UpdateAchatRequest
    {
        [Required(ErrorMessage = "L'ID de l'achat est requis")]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateAchat { get; set; }

        [RegularExpression("^(Poussins|Aliment|Medicament|Materiel|Services|Transport|Autre)$", 
            ErrorMessage = "Type d'achat invalide")]
        public string? TypeAchat { get; set; }

        [MaxLength(500, ErrorMessage = "La description ne peut dépasser 500 caractères")]
        public string? Description { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "La quantité doit être entre 0.01 et 1 000 000")]
        public decimal? Quantite { get; set; }

        [RegularExpression("^(Unite|Kg|L|Sac|Dose|Heure|Km|Paquet)$", 
            ErrorMessage = "Unité de mesure invalide")]
        public string? UniteMesure { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Le prix unitaire doit être entre 0.01 et 1 000 000")]
        public decimal? PrixUnitaire { get; set; }

        [MaxLength(50, ErrorMessage = "Le numéro de facture ne peut dépasser 50 caractères")]
        public string? FactureNumero { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateReception { get; set; }

        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent dépasser 2000 caractères")]
        public string? Observations { get; set; }
    }
}