using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;
using Domain.Enums;

namespace Application.Features.Clients.Queries.GetAllClients
{
    /// <summary>
    /// Handler pour récupérer tous les clients avec filtres optionnels.
    /// </summary>
    public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, List<ClientResponseDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllClientsQueryHandler> _logger;

        public GetAllClientsQueryHandler(
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<GetAllClientsQueryHandler> logger)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ClientResponseDto>> Handle(
            GetAllClientsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des clients");

            // ============================================
            // 1. Récupérer les clients selon les filtres
            // ============================================
            IEnumerable<Domain.Entities.Client> clients;

            // Filtre par type
            if (!string.IsNullOrEmpty(request.TypeClient))
            {
                if (Enum.TryParse<TypeClient>(request.TypeClient, true, out var type))
                {
                    clients = await _clientRepository.GetByTypeAsync(type, cancellationToken);
                }
                else
                {
                    return new List<ClientResponseDto>();
                }
            }
            // Filtre par statut actif uniquement
            else if (request.OnlyActifs.HasValue && request.OnlyActifs.Value)
            {
                clients = await _clientRepository.GetActifsAsync(cancellationToken);
            }
            // Recherche par terme
            else if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                clients = await _clientRepository.GetByNomAsync(request.SearchTerm, cancellationToken);
            }
            // Tous les clients
            else
            {
                clients = await _clientRepository.GetAllAsync(cancellationToken);
            }

            // ============================================
            // 2. Mapper et retourner
            // ============================================
            return _mapper.Map<List<ClientResponseDto>>(clients);
        }
    }
}