// Application/Features/Bandes/Queries/GetBandeStats/GetBandeStatsQueryHandler.cs
using MediatR;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;

namespace Application.Features.Bandes.Queries.GetBandeStats
{
    /// <summary>
    /// Handler pour récupérer les statistiques des bandes.
    /// </summary>
    public class GetBandeStatsQueryHandler : IRequestHandler<GetBandeStatsQuery, BandeStatsDto>
    {
        private readonly IBandeRepository _bandeRepository;

        public GetBandeStatsQueryHandler(IBandeRepository bandeRepository)
        {
            _bandeRepository = bandeRepository;
        }

        public async Task<BandeStatsDto> Handle(
            GetBandeStatsQuery request, 
            CancellationToken cancellationToken)
        {
            // Utilise la méthode GetStatsAsync que vous avez déjà dans le repository
            var stats = await _bandeRepository.GetStatsAsync(cancellationToken);
            
            return stats;
        }
    }
}