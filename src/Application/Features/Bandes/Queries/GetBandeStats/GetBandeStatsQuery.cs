using MediatR;
using Application.Features.Bandes.Dtos;

namespace Application.Features.Bandes.Queries.GetBandeStats
{
    /// <summary>
    /// Query pour récupérer les statistiques des bandes.
    /// </summary>
    public record GetBandeStatsQuery : IRequest<BandeStatsDto>
    {
        // Pas de paramètres pour l'instant; On pourrait ajouter des filtres plus tard (par période, année, etc.)
    }
}