using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Alertes
{
    public class UpdateAlerteRequest
    {
        [Required(ErrorMessage = "L'ID de l'alerte est requis")]
        public int Id { get; set; }

        public bool? EstLue { get; set; }

        public int? UtilisateurId { get; set; }
    }
}