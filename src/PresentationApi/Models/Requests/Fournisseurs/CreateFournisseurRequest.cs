using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Fournisseurs
{
    public class CreateFournisseurRequest
    {
        [Required(ErrorMessage = "Le nom du fournisseur est requis")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de fournisseur est requis")]
        [RegularExpression("^(Poussiniere|Aliment|Medicament|Materiel|Services|Transport|Autre)$", 
            ErrorMessage = "Type de fournisseur invalide")]
        public string TypeFournisseur { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Le téléphone ne peut dépasser 20 caractères")]
        [RegularExpression(@"^[0-9+\-\s]+$", ErrorMessage = "Format de téléphone invalide")]
        public string? Telephone { get; set; }

        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide")]
        [MaxLength(100, ErrorMessage = "L'email ne peut dépasser 100 caractères")]
        public string? Email { get; set; }

        [MaxLength(500, ErrorMessage = "L'adresse ne peut dépasser 500 caractères")]
        public string? Adresse { get; set; }

        [MaxLength(100, ErrorMessage = "Le nom du contact ne peut dépasser 100 caractères")]
        public string? ContactPersonne { get; set; }

        [MaxLength(2000, ErrorMessage = "Les notes ne peuvent dépasser 2000 caractères")]
        public string? Notes { get; set; }
    }
}