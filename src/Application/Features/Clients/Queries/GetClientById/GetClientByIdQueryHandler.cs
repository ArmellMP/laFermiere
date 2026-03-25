using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;

namespace Application.Features.Clients.Queries.GetClientById
{
    /// <summary>
    /// Handler pour récupérer un client par son ID.
    /// </summary>
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientResponseDto>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClientByIdQueryHandler> _logger;

        public GetClientByIdQueryHandler(
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<GetClientByIdQueryHandler> logger)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
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
            var clientDto = _mapper.Map<ClientResponseDto>(client);

            // ============================================
            // 3. Ajouter les informations de ventes si demandé
            // ============================================
            if (request.IncludeVentes)
            {
                // TODO: Récupérer le nombre de ventes et le montant total
                // clientDto.NombreVentes = await _venteRepository.GetCountByClientIdAsync(request.Id);
                // clientDto.MontantTotalAchats = await _venteRepository.GetTotalByClientIdAsync(request.Id);
            }

            return clientDto;
        }
    }
}