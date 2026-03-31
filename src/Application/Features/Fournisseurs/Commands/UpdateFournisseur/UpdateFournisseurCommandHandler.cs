using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Fournisseurs.Commands.UpdateFournisseur
{
    public class UpdateFournisseurCommandHandler : IRequestHandler<UpdateFournisseurCommand, FournisseurResponseDto>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateFournisseurCommandHandler> _logger;

        public UpdateFournisseurCommandHandler(
            IFournisseurRepository fournisseurRepository,
            IMapper mapper,
            ILogger<UpdateFournisseurCommandHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FournisseurResponseDto> Handle(
            UpdateFournisseurCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateFournisseurDto;

            _logger.LogInformation("Mise à jour du fournisseur {FournisseurId}", updateDto.Id);

            // 1. Récupérer le fournisseur existant
            var existingFournisseur = await _fournisseurRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingFournisseur == null)
            {
                throw new NotFoundException($"Fournisseur avec l'ID {updateDto.Id} non trouvé.");
            }

            // 2. Mise à jour des propriétés
            if (!string.IsNullOrEmpty(updateDto.Nom))
                existingFournisseur.Nom = updateDto.Nom;

            if (!string.IsNullOrEmpty(updateDto.TypeFournisseur))
            {
                if (Enum.TryParse<TypeFournisseur>(updateDto.TypeFournisseur, true, out var type))
                {
                    existingFournisseur.TypeFournisseur = type;
                }
            }

            existingFournisseur.Telephone = updateDto.Telephone ?? existingFournisseur.Telephone;
            existingFournisseur.Email = updateDto.Email ?? existingFournisseur.Email;
            existingFournisseur.Adresse = updateDto.Adresse ?? existingFournisseur.Adresse;
            existingFournisseur.ContactPersonne = updateDto.ContactPersonne ?? existingFournisseur.ContactPersonne;
            existingFournisseur.Notes = updateDto.Notes ?? existingFournisseur.Notes;

            if (updateDto.EstActif.HasValue)
                existingFournisseur.EstActif = updateDto.EstActif.Value;

            existingFournisseur.UpdatedAt = DateTime.UtcNow;

            // 3. Sauvegarde
            await _fournisseurRepository.UpdateAsync(existingFournisseur, cancellationToken);

            _logger.LogInformation("Fournisseur {FournisseurId} mis à jour avec succès", updateDto.Id);

            // 4. Retour de la réponse
            return _mapper.Map<FournisseurResponseDto>(existingFournisseur);
        }
    }
}