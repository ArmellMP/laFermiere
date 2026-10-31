using MediatR;

namespace Application.Features.Achats.Queries.GetAchatsStats
{
    public record GetAchatsStatsQuery : IRequest<AchatsStatsDto>
    {
        public int? BandeId { get; init; }
        public int? FournisseurId { get; init; }
        public DateTime? DateDebut { get; init; }
        public DateTime? DateFin { get; init; }
    }

    public class AchatsStatsDto
    {
        public int TotalAchats { get; set; }
        public decimal MontantTotal { get; set; }
        
        // Statistiques par type
        public Dictionary<string, decimal> MontantParType { get; set; } = new();
        public Dictionary<string, decimal> QuantiteParType { get; set; } = new();
        
        // Statistiques par fournisseur
        public Dictionary<string, decimal> MontantParFournisseur { get; set; } = new();
        
        // Achats par statut de réception
        public int AchatsRecus { get; set; }
        public int AchatsNonRecus { get; set; }
        public decimal MontantRecus { get; set; }
        public decimal MontantNonRecus { get; set; }
    }
}