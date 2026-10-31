using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Ventes.Queries.GetAllVentes
{
    public class GetAllVentesQueryHandler : IRequestHandler<GetAllVentesQuery, List<VenteResponseDto>>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly ILogger<GetAllVentesQueryHandler> _logger;

        public GetAllVentesQueryHandler(
            IVenteRepository venteRepository,
            ILogger<GetAllVentesQueryHandler> logger)
        {
            _venteRepository = venteRepository;
            _logger = logger;
        }

        public async Task<List<VenteResponseDto>> Handle(
            GetAllVentesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des ventes");

            IEnumerable<Domain.Entities.Vente> ventes;

            // Filtrer par date
            if (request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                ventes = await _venteRepository.GetByDateRangeAsync(
                    request.DateDebut.Value, 
                    request.DateFin.Value, 
                    cancellationToken);
            }
            // Filtrer par bande
            else if (request.BandeId.HasValue)
            {
                ventes = await _venteRepository.GetByBandeIdAsync(request.BandeId.Value, cancellationToken);
            }
            // Filtrer par client
            else if (request.ClientId.HasValue)
            {
                ventes = await _venteRepository.GetByClientIdAsync(request.ClientId.Value, cancellationToken);
            }
            // Toutes les ventes
            else
            {
                ventes = await _venteRepository.GetAllAsync(cancellationToken);
            }

            // Appliquer les filtres supplémentaires en mémoire
            if (!string.IsNullOrEmpty(request.TypeVente))
            {
                ventes = ventes.Where(v => v.TypeVente.ToString() == request.TypeVente);
            }

            if (request.EstPaye.HasValue)
            {
                ventes = ventes.Where(v => v.EstPaye == request.EstPaye.Value);
            }

            return ventes.ToResponseDtos();
        }
    }
}