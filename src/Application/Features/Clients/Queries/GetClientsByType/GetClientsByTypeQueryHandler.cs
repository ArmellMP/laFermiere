using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Clients.Queries.GetClientsByType
{
    /// <summary>
    /// Handler pour récupérer les clients par type.
    /// </summary>
    public class GetClientsByTypeQueryHandler : IRequestHandler<GetClientsByTypeQuery, List<ClientResponseDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<GetClientsByTypeQueryHandler> _logger;

        public GetClientsByTypeQueryHandler(
            IClientRepository clientRepository,
            ILogger<GetClientsByTypeQueryHandler> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<List<ClientResponseDto>> Handle(
            GetClientsByTypeQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des clients de type {TypeClient}", request.TypeClient);

            // ============================================
            // 1. Valider le type de client
            // ============================================
            if (!Enum.TryParse<TypeClient>(request.TypeClient, true, out var typeClient))
            {
                throw new BusinessRuleException(
                    $"Type de client invalide : {request.TypeClient}. " +
                    "Valeurs acceptées : Particulier, Restaurant, Marche, Grossiste, Transformateur, Autre");
            }

            // ============================================
            // 2. Récupérer les clients par type
            // ============================================
            var clients = await _clientRepository.GetByTypeAsync(typeClient, cancellationToken);
            
            // ============================================
            // 3. Filtrer par statut actif si demandé
            // ============================================
            if (request.OnlyActifs)
            {
                clients = clients.Where(c => c.EstActif);
            }

            // ============================================
            // 4. Mapper et retourner
            // ============================================
            return clients.ToResponseDtos();
        }
    }
}