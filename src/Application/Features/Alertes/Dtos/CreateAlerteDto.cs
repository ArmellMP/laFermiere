namespace Application.Features.Alertes.Dtos
{
    public class CreateAlerteDto
    {
        public string TypeAlerte { get; set; } = string.Empty;
        public int EntiteId { get; set; }
        public string Niveau { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}