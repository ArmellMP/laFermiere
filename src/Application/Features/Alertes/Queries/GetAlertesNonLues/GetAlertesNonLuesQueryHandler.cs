using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Features.Alertes.Interfaces;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Queries.GetAlertesNonLues
{
    public class GetAlertesNonLuesQueryHandler : IRequestHandler<GetAlertesNonLuesQuery, List<AlerteResponseDto>>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAlertesNonLuesQueryHandler> _logger;

        public GetAlertesNonLuesQueryHandler(
            IAlerteRepository alerteRepository,
            IMapper mapper,
            ILogger<GetAlertesNonLuesQueryHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<AlerteResponseDto>> Handle(
            GetAlertesNonLuesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des alertes non lues");

            var alertes = await _alerteRepository.GetNonLuesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.TypeAlerte))
            {
                alertes = alertes.Where(a => a.TypeAlerte.ToString() == request.TypeAlerte);
            }

            if (!string.IsNullOrEmpty(request.Niveau))
            {
                alertes = alertes.Where(a => a.Niveau.ToString() == request.Niveau);
            }

            return _mapper.Map<List<AlerteResponseDto>>(alertes);
        }
    }
}