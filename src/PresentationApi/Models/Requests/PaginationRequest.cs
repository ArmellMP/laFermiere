using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests
{
    /// <summary>
    /// Paramètres de pagination lus dans l'URL : ?page=2&amp;taille=50.
    /// Absents : première page de 20 éléments.
    /// Valeur hors limites : réponse 400 au format ErrorResponse (validation DataAnnotations).
    /// </summary>
    public class PaginationRequest
    {
        public const int TailleParDefaut = 20;
        public const int TailleMax = 100;

        /// <summary>Numéro de page, à partir de 1.</summary>
        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de page doit être supérieur ou égal à 1.")]
        public int Page { get; set; } = 1;

        /// <summary>Nombre d'éléments par page (1 à 100).</summary>
        [Range(1, TailleMax, ErrorMessage = "La taille de page doit être comprise entre 1 et 100.")]
        public int Taille { get; set; } = TailleParDefaut;
    }
}
