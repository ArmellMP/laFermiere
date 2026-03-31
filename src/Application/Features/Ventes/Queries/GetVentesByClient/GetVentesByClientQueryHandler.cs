using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetVentesByClient
{
    public class GetVentesByClientQueryHandler : IRequestHandler<GetVentesByClientQuery, List<VenteResponseDto>>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVentesByClientQueryHandler> _logger;

        public GetVentesByClientQueryHandler(
            IVenteRepository venteRepository,
            IMapper mapper,
            ILogger<GetVentesByClientQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<VenteResponseDto>> Handle(
            GetVentesByClientQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des ventes du client {ClientId}", request.ClientId);

            var ventes = await _venteRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
            
            return _mapper.Map<List<VenteResponseDto>>(ventes);
        }
    }
}