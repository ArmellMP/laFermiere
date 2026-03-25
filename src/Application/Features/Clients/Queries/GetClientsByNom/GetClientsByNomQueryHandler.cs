using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Queries.GetClientsByNom
{
    /// <summary>
    /// Handler pour rechercher des clients par nom.
    /// </summary>
    public class GetClientsByNomQueryHandler : IRequestHandler<GetClientsByNomQuery, List<ClientResponseDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClientsByNomQueryHandler> _logger;

        public GetClientsByNomQueryHandler(
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<GetClientsByNomQueryHandler> logger)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ClientResponseDto>> Handle(
            GetClientsByNomQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recherche de clients avec le terme '{SearchTerm}'", request.SearchTerm);

            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return new List<ClientResponseDto>();
            }

            // ============================================
            // 1. Récupérer les clients par nom
            // ============================================
            var clients = await _clientRepository.GetByNomAsync(request.SearchTerm, cancellationToken);
            
            // ============================================
            // 2. Limiter les résultats
            // ============================================
            var limitedClients = clients.Take(request.Limit);
            
            // ============================================
            // 3. Mapper et retourner
            // ============================================
            return _mapper.Map<List<ClientResponseDto>>(limitedClients);
        }
    }
}