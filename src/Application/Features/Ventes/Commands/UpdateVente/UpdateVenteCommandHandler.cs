using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Ventes.Commands.UpdateVente
{
    public class UpdateVenteCommandHandler : IRequestHandler<UpdateVenteCommand, VenteResponseDto>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateVenteCommandHandler> _logger;

        public UpdateVenteCommandHandler(
            IVenteRepository venteRepository,
            IMapper mapper,
            ILogger<UpdateVenteCommandHandler> logger)
        {
            _venteRepository = venteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<VenteResponseDto> Handle(
            UpdateVenteCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateVenteDto;

            _logger.LogInformation("Mise à jour de la vente {VenteId}", updateDto.Id);

            // 1. Récupérer la vente existante
            var existingVente = await _venteRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingVente == null)
            {
                throw new NotFoundException($"Vente avec l'ID {updateDto.Id} non trouvée.");
            }

            // 2. Mise à jour des propriétés
            if (updateDto.DateVente.HasValue)
                existingVente.DateVente = updateDto.DateVente.Value;

            if (!string.IsNullOrEmpty(updateDto.TypeVente))
            {
                if (Enum.TryParse<TypeVente>(updateDto.TypeVente, true, out var typeVente))
                {
                    existingVente.TypeVente = typeVente;
                }
            }

            if (updateDto.Quantite.HasValue)
                existingVente.Quantite = updateDto.Quantite.Value;

            if (!string.IsNullOrEmpty(updateDto.UniteMesure))
            {
                if (Enum.TryParse<UniteMesureVente>(updateDto.UniteMesure, true, out var uniteMesure))
                {
                    existingVente.UniteMesure = uniteMesure;
                }
            }

            if (updateDto.PrixUnitaire.HasValue)
            {
                existingVente.PrixUnitaire = updateDto.PrixUnitaire.Value;
                // Recalculer le montant total
                existingVente.MontantTotal = existingVente.Quantite * existingVente.PrixUnitaire;
            }

            if (!string.IsNullOrEmpty(updateDto.ModePaiement))
            {
                if (Enum.TryParse<ModePaiement>(updateDto.ModePaiement, true, out var modePaiement))
                {
                    existingVente.ModePaiement = modePaiement;
                }
            }

            if (updateDto.EstPaye.HasValue)
                existingVente.EstPaye = updateDto.EstPaye.Value;

            existingVente.DatePaiement = updateDto.DatePaiement ?? existingVente.DatePaiement;
            existingVente.Observations = updateDto.Observations ?? existingVente.Observations;

            // 3. Sauvegarde
            await _venteRepository.UpdateAsync(existingVente, cancellationToken);

            _logger.LogInformation("Vente {VenteId} mise à jour avec succès", updateDto.Id);

            // 4. Retour de la réponse
            return _mapper.Map<VenteResponseDto>(existingVente);
        }
    }
}