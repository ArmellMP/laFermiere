using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetVentesByBande
{
    public class GetVentesByBandeQueryHandler : IRequestHandler<GetVentesByBandeQuery, List<VenteResponseDto>>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVentesByBandeQueryHandler> _logger;

        public GetVentesByBandeQueryHandler(
            IVenteRepository venteRepository,
            IMapper mapper,
            ILogger<GetVentesByBandeQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<VenteResponseDto>> Handle(
            GetVentesByBandeQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des ventes de la bande {BandeId}", request.BandeId);

            var ventes = await _venteRepository.GetByBandeIdAsync(request.BandeId, cancellationToken);
            
            return _mapper.Map<List<VenteResponseDto>>(ventes);
        }
    }
}