using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Ventes.Queries.GetVentesByBande
{
    public class GetVentesByBandeQueryHandler : IRequestHandler<GetVentesByBandeQuery, List<VenteResponseDto>>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly ILogger<GetVentesByBandeQueryHandler> _logger;

        public GetVentesByBandeQueryHandler(
            IVenteRepository venteRepository,
            ILogger<GetVentesByBandeQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _logger = logger;
        }

        public async Task<List<VenteResponseDto>> Handle(
            GetVentesByBandeQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des ventes de la bande {BandeId}", request.BandeId);

            var ventes = await _venteRepository.GetByBandeIdAsync(request.BandeId, cancellationToken);
            
            return ventes.ToResponseDtos();
        }
    }
}