using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Alertes
{
    public class MarquerAlertesRequest
    {
        public List<int>? AlerteIds { get; set; }
        
        /// <summary>
        /// Ignoré : l'alerte est marquée au nom de l'utilisateur connecté (son jeton).
        /// Conservé pour ne pas casser les clients qui l'envoient encore.
        /// </summary>
        public int? UtilisateurId { get; set; }
        
        public bool MarquerToutes { get; set; }
    }
}