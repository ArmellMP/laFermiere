using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Alertes.Interfaces;
using Application.Features.Alertes.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Alertes.Queries.GetAlerteById
{
    public class GetAlerteByIdQueryHandler : IRequestHandler<GetAlerteByIdQuery, AlerteResponseDto>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<GetAlerteByIdQueryHandler> _logger;

        public GetAlerteByIdQueryHandler(
            IAlerteRepository alerteRepository,
            ILogger<GetAlerteByIdQueryHandler> logger)
        {
            _alerteRepository = alerteRepository;
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

            return alerte.ToResponseDto();
        }
    }
}