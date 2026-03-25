using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Clients.Interfaces;
using Application.Features.Clients.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Clients.Commands.UpdateClient
{
    /// <summary>
    /// Handler pour mettre à jour un client existant.
    /// </summary>
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientResponseDto>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateClientCommandHandler> _logger;

        public UpdateClientCommandHandler(
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<UpdateClientCommandHandler> logger)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ClientResponseDto> Handle(
            UpdateClientCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateClientDto;

            _logger.LogInformation("Mise à jour du client {ClientId}", updateDto.Id);

            // ============================================
            // 1. Récupérer le client existant
            // ============================================
            var existingClient = await _clientRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingClient == null)
            {
                throw new NotFoundException($"Client avec l'ID {updateDto.Id} non trouvé.");
            }

            // ============================================
            // 2. Vérifier l'unicité de l'email (si changement)
            // ============================================
            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingClient.Email)
            {
                var emailExists = await _clientRepository.EmailExistsAsync(
                    updateDto.Email, 
                    updateDto.Id, 
                    cancellationToken);
                    
                if (emailExists)
                {
                    throw new BusinessRuleException(
                        $"Un client avec l'email '{updateDto.Email}' existe déjà.");
                }
            }

            // ============================================
            // 3. Mise à jour des propriétés
            // ============================================
            if (!string.IsNullOrEmpty(updateDto.Nom))
                existingClient.Nom = updateDto.Nom;

            if (!string.IsNullOrEmpty(updateDto.TypeClient))
            {
                if (Enum.TryParse<TypeClient>(updateDto.TypeClient, true, out var typeClient))
                {
                    existingClient.TypeClient = typeClient;
                }
            }

            existingClient.Telephone = updateDto.Telephone ?? existingClient.Telephone;
            existingClient.Email = updateDto.Email ?? existingClient.Email;
            existingClient.Adresse = updateDto.Adresse ?? existingClient.Adresse;
            existingClient.Notes = updateDto.Notes ?? existingClient.Notes;

            if (updateDto.EstActif.HasValue)
                existingClient.EstActif = updateDto.EstActif.Value;

            existingClient.UpdatedAt = DateTime.UtcNow;

            // ============================================
            // 4. Sauvegarde
            // ============================================
            await _clientRepository.UpdateAsync(existingClient, cancellationToken);

            _logger.LogInformation("Client {ClientId} mis à jour avec succès", updateDto.Id);

            // ============================================
            // 5. Retour de la réponse
            // ============================================
            return _mapper.Map<ClientResponseDto>(existingClient);
        }
    }
}