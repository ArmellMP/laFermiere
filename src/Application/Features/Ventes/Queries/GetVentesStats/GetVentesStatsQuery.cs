using MediatR;

namespace Application.Features.Ventes.Queries.GetVentesStats
{
    public record GetVentesStatsQuery : IRequest<VentesStatsDto>
    {
        public int? BandeId { get; init; }
        public DateTime? DateDebut { get; init; }
        public DateTime? DateFin { get; init; }
    }

    public class VentesStatsDto
    {
        public int TotalVentes { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal MontantPaye { get; set; }
        public decimal MontantImpaye { get; set; }
        public int NombreVentesPayees { get; set; }
        public int NombreVentesImpayees { get; set; }
        
        // Statistiques par type de vente
        public Dictionary<string, decimal> MontantParType { get; set; } = new();
        public Dictionary<string, int> QuantiteParType { get; set; } = new();
        
        // Statistiques par mode de paiement
        public Dictionary<string, decimal> MontantParModePaiement { get; set; } = new();
    }
}