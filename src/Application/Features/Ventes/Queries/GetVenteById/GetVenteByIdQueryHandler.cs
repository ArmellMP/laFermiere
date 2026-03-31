using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetVenteById
{
    public class GetVenteByIdQueryHandler : IRequestHandler<GetVenteByIdQuery, VenteResponseDto>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVenteByIdQueryHandler> _logger;

        public GetVenteByIdQueryHandler(
            IVenteRepository venteRepository,
            IMapper mapper,
            ILogger<GetVenteByIdQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<VenteResponseDto> Handle(
            GetVenteByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la vente {VenteId}", request.Id);

            var vente = await _venteRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (vente == null)
            {
                throw new NotFoundException($"Vente avec l'ID {request.Id} non trouvée.");
            }

            return _mapper.Map<VenteResponseDto>(vente);
        }
    }
}