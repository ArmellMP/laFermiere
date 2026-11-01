// Application/Features/Alertes/Queries/GetAlertesStats/GetAlertesStatsQuery.cs
using MediatR;

namespace Application.Features.Alertes.Queries.GetAlertesStats
{
    public record GetAlertesStatsQuery : IRequest<AlertesStatsDto>
    {
        public DateTime? DateDebut { get; init; }
        public DateTime? DateFin { get; init; }
    }

    public class AlertesStatsDto
    {
        public int TotalAlertes { get; set; }
        public int AlertesNonLues { get; set; }
        public int AlertesLues { get; set; }
        
        // Statistiques par type
        public Dictionary<string, int> CountParType { get; set; } = new();
        
        // Statistiques par niveau
        public Dictionary<string, int> CountParNiveau { get; set; } = new();
        
        // Évolution temporelle
        public Dictionary<string, int> EvolutionParJour { get; set; } = new();
    }
}