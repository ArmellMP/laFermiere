namespace Application.Features.Alertes.Dtos
{
    public class UpdateAlerteDto
    {
        public int Id { get; set; }
        public bool? EstLue { get; set; }
        public int? UtilisateurId { get; set; }
    }
}