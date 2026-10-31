using System.ComponentModel.DataAnnotations;

namespace PresentationApi.Models.Requests.Bandes
{
    public class CloturerBandeRequest
    {
        [Required(ErrorMessage = "L'ID de la bande est requis")]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateCloture { get; set; }

        [MaxLength(2000, ErrorMessage = "Les observations ne peuvent dépasser 2000 caractères")]
        public string? ObservationsCloture { get; set; }
    }
}