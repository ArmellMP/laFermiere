using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Alertes
{
    public class MarquerAlertesRequest
    {
        public List<int>? AlerteIds { get; set; }
        
        [Required(ErrorMessage = "L'ID de l'utilisateur est requis")]
        public int UtilisateurId { get; set; }
        
        public bool MarquerToutes { get; set; }
    }
}