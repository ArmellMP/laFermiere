using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Alertes.Interfaces;
using Application.Features.Alertes.Dtos;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Alertes.Queries.GetAllAlertes
{
    public class GetAllAlertesQueryHandler : IRequestHandler<GetAllAlertesQuery, List<AlerteResponseDto>>
    {
        private readonly IAlerteRepository _alerteRepository;
        private readonly ILogger<GetAllAlertesQueryHandler> _logger;

        public GetAllAlertesQueryHandler(
            IAlerteRepository alerteRepository,
            ILogger<GetAllAlertesQueryHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _logger = logger;
        }

        public async Task<List<AlerteResponseDto>> Handle(
            GetAllAlertesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des alertes");

            IEnumerable<Domain.Entities.Alerte> alertes;

            // Filtrer par période
            if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                alertes = await _alerteRepository.GetByDateRangeAsync(
                    request.DateDebut.Value,
                    request.DateFin.Value,
                    cancellationToken);
            }
            // Filtrer par entité
            else if (request.EntiteId.HasValue)
            {
                alertes = await _alerteRepository.GetByEntiteAsync(request.EntiteId.Value, cancellationToken);
            }
            // Filtrer par statut non lu
            else if (request.OnlyNonLues.HasValue && request.OnlyNonLues.Value)
            {
                alertes = await _alerteRepository.GetNonLuesAsync(cancellationToken);
            }
            // Toutes les alertes
            else
            {
                alertes = await _alerteRepository.GetAllAsync(cancellationToken);
            }

            // Appliquer les filtres supplémentaires en mémoire
            if (!string.IsNullOrEmpty(request.TypeAlerte))
            {
                alertes = alertes.Where(a => a.TypeAlerte.ToString() == request.TypeAlerte);
            }

            if (!string.IsNullOrEmpty(request.Niveau))
            {
                alertes = alertes.Where(a => a.Niveau.ToString() == request.Niveau);
            }

            return alertes.ToResponseDtos();
        }
    }
}