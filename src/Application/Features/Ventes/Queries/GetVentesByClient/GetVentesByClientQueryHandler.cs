using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Ventes.Queries.GetVentesByClient
{
    public class GetVentesByClientQueryHandler : IRequestHandler<GetVentesByClientQuery, List<VenteResponseDto>>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly ILogger<GetVentesByClientQueryHandler> _logger;

        public GetVentesByClientQueryHandler(
            IVenteRepository venteRepository,
            ILogger<GetVentesByClientQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _logger = logger;
        }

        public async Task<List<VenteResponseDto>> Handle(
            GetVentesByClientQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des ventes du client {ClientId}", request.ClientId);

            var ventes = await _venteRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
            
            return ventes.ToResponseDtos();
        }
    }
}