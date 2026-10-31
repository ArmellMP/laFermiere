using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Alertes.Interfaces;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Queries.GetAlerteById
{
    public class GetAlerteByIdQueryHandler : IRequestHandler<GetAlerteByIdQuery, AlerteResponseDto>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAlerteByIdQueryHandler> _logger;

        public GetAlerteByIdQueryHandler(
            IAlerteRepository alerteRepository,
            IMapper mapper,
            ILogger<GetAlerteByIdQueryHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AlerteResponseDto> Handle(
            GetAlerteByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de l'alerte {AlerteId}", request.Id);

            var alerte = await _alerteRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (alerte == null)
            {
                throw new NotFoundException($"Alerte avec l'ID {request.Id} non trouvée.");
            }

            return _mapper.Map<AlerteResponseDto>(alerte);
        }
    }
}