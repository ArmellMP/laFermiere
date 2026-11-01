using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Clients.Commands.CreateClient
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientResponseDto>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<CreateClientCommandHandler> _logger;

        public CreateClientCommandHandler(
            IClientRepository clientRepository,
            ILogger<CreateClientCommandHandler> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<ClientResponseDto> Handle(
            CreateClientCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateClientDto;

            _logger.LogInformation("Création d'un nouveau client : {Nom}", createDto.Nom);

            // ============================================
            // 1. Vérifier que l'email n'existe pas déjà (si fourni)
            // ============================================
            if (!string.IsNullOrEmpty(createDto.Email))
            {
                var emailExists = await _clientRepository.EmailExistsAsync(
                    createDto.Email, 
                    null,  // excludeId (pas d'exclusion pour la création)
                    cancellationToken);
                    
                if (emailExists)
                {
                    throw new BusinessRuleException(
                        $"Un client avec l'email '{createDto.Email}' existe déjà.");
                }
            }

            // ============================================
            // 2. Conversion du type de client (string → enum)
            // ============================================
            if (!Enum.TryParse<TypeClient>(createDto.TypeClient, true, out var typeClient))
            {
                throw new BusinessRuleException($"Type de client invalide : {createDto.TypeClient}");
            }

            // ============================================
            // 3. Création de l'entité
            // ============================================
            var client = new Client
            {
                Nom = createDto.Nom,
                TypeClient = typeClient,
                Telephone = createDto.Telephone,
                Email = createDto.Email,
                Adresse = createDto.Adresse,
                Notes = createDto.Notes,
                EstActif = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // ============================================
            // 4. Sauvegarde
            // ============================================
            var createdClient = await _clientRepository.AddAsync(client, cancellationToken);

            _logger.LogInformation("Client créé avec succès : ID {ClientId}", createdClient.Id);

            // ============================================
            // 5. Retour de la réponse
            // ============================================
            var response = new ClientResponseDto
            {
                Id = createdClient.Id,
                Nom = createdClient.Nom,
                TypeClient = createdClient.TypeClient.ToString(),
                TypeClientLibelle = createdClient.TypeClientLibelle,
                Telephone = createdClient.Telephone,
                Email = createdClient.Email,
                Adresse = createdClient.Adresse,
                Notes = createdClient.Notes,
                EstActif = createdClient.EstActif,
                CreatedAt = createdClient.CreatedAt,
                UpdatedAt = createdClient.UpdatedAt
            };
            return response;
        }
    }
}