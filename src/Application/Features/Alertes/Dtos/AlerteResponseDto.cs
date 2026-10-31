namespace Application.Features.Alertes.Dtos
{
    public class AlerteResponseDto
    {
        public int Id { get; set; }
        public string TypeAlerte { get; set; } = string.Empty;
        public string TypeAlerteLibelle { get; set; } = string.Empty;
        public int EntiteId { get; set; }
        public string Niveau { get; set; } = string.Empty;
        public string NiveauLibelle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool EstLue { get; set; }
        public DateTime? DateLecture { get; set; }
        public DateTime DateAlerte { get; set; }
        public int? UtilisateurId { get; set; }
        public string? UtilisateurNom { get; set; }  // Rempli via JOIN
    }
}