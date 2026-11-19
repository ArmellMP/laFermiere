using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;
using Application.Features.Ventes.Interfaces;
using Application.Common.Mappings;

namespace Application.Features.Clients.Queries.GetClientById
{
    /// <summary>
    /// Handler pour récupérer un client par son ID.
    /// </summary>
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientResponseDto>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IVenteRepository _venteRepository;
        private readonly ILogger<GetClientByIdQueryHandler> _logger;

        public GetClientByIdQueryHandler(
            IClientRepository clientRepository,
            IVenteRepository venteRepository,
            ILogger<GetClientByIdQueryHandler> logger)
        {
            _clientRepository = clientRepository;
            _venteRepository = venteRepository;
            _logger = logger;
        }

        public async Task<ClientResponseDto> Handle(
            GetClientByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du client {ClientId}", request.Id);

            // ============================================
            // 1. Récupérer le client
            // ============================================
            var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (client == null)
            {
                throw new NotFoundException($"Client avec l'ID {request.Id} non trouvé.");
            }

            // ============================================
            // 2. Mapper en DTO
            // ============================================
            var clientDto = client.ToResponseDto();

            // ============================================
            // 3. Statistiques de ventes du client (une seule requête d'agrégat)
            // ============================================
            var (nombreVentes, montantTotal) = await _venteRepository.GetStatsByClientAsync(request.Id, cancellationToken);
            clientDto.NombreVentes = nombreVentes;
            clientDto.MontantTotalAchats = montantTotal;   // ce que le client nous a acheté

            return clientDto;
        }
    }
}