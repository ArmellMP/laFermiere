using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Alertes
{
    public class UpdateAlerteRequest
    {
        [Required(ErrorMessage = "L'ID de l'alerte est requis")]
        public int Id { get; set; }

        public bool? EstLue { get; set; }

        /// <summary>Ignoré : l'alerte est marquée au nom de l'utilisateur connecté (son jeton).</summary>
        public int? UtilisateurId { get; set; }
    }
}