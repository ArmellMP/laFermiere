using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Bandes.Interfaces;
using Application.Features.Ventes.Interfaces;
using Application.Features.Ventes.Dtos;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Ventes.Commands.UpdateVente
{
    public class UpdateVenteCommandHandler : IRequestHandler<UpdateVenteCommand, VenteResponseDto>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly ILogger<UpdateVenteCommandHandler> _logger;

        public UpdateVenteCommandHandler(
            IVenteRepository venteRepository,
            IBandeRepository bandeRepository,
            ILogger<UpdateVenteCommandHandler> logger)
        {
            _venteRepository = venteRepository;
            _bandeRepository = bandeRepository;
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

            // Nombre de poulets que cette vente retirait de la bande AVANT modification
            // (à retenir maintenant : les propriétés vont être modifiées juste après)
            var pouletsRetiresAvant = PouletsRetiresDeLaBande(existingVente);

            // 2. Mise à jour des propriétés (un champ null = "ne pas modifier")
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
                existingVente.PrixUnitaire = updateDto.PrixUnitaire.Value;

            // Le montant total est TOUJOURS recalculé, quel que soit le champ modifié.
            // (Avant : seulement si le prix était envoyé. Modifier la quantité seule laissait
            //  l'ancien montant, et la contrainte SQL chk_montant_coherent
            //  "MontantTotal = Quantite * PrixUnitaire" refusait la mise à jour => erreur 500.)
            existingVente.MontantTotal = existingVente.Quantite * existingVente.PrixUnitaire;

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

            // 3. Impact sur l'effectif de la bande
            // La création retire les poulets vendus de la bande, la suppression les restitue.
            // La modification doit donc appliquer la DIFFÉRENCE, sinon le compteur se décale :
            // ex. vente 10 -> 12 sans ajustement, puis suppression qui restitue 12 => 2 poulets "créés".
            var pouletsRetiresApres = PouletsRetiresDeLaBande(existingVente);
            var difference = pouletsRetiresApres - pouletsRetiresAvant;   // > 0 : on retire plus de poulets

            Bande? bande = null;
            if (difference != 0)
            {
                bande = await _bandeRepository.GetByIdAsync(existingVente.BandeId, cancellationToken)
                    ?? throw new NotFoundException($"Bande avec l'ID {existingVente.BandeId} non trouvée.");

                // Vérifier AVANT d'écrire quoi que ce soit : on ne vend pas des poulets qu'on n'a pas
                if (difference > 0 && bande.NbPoussinsActuel < difference)
                {
                    throw new BusinessRuleException(
                        $"Effectif insuffisant. Poulets disponibles : {bande.NbPoussinsActuel}, " +
                        $"poulets supplémentaires demandés : {difference}");
                }

                bande.NbPoussinsActuel -= difference;
                bande.UpdatedAt = DateTime.UtcNow;
            }

            // 4. Sauvegarde : la vente, puis la bande si son effectif a changé
            await _venteRepository.UpdateAsync(existingVente, cancellationToken);

            if (bande != null)
            {
                await _bandeRepository.UpdateAsync(bande, cancellationToken);
                _logger.LogInformation(
                    "Bande {BandeId} : effectif ajusté de {Difference} suite à la modification de la vente {VenteId}",
                    bande.Id, -difference, existingVente.Id);
            }

            _logger.LogInformation("Vente {VenteId} mise à jour avec succès", updateDto.Id);

            // 5. Retour de la réponse
            return existingVente.ToResponseDto();
        }

        /// <summary>
        /// Nombre de poulets qu'une vente retire de l'effectif de sa bande.
        /// Même règle que la création et la suppression : seules les ventes de poulets
        /// (vivants, nettoyés, fumés) comptent ; fientes et autres produits = 0.
        /// </summary>
        private static int PouletsRetiresDeLaBande(Vente vente)
        {
            return vente.TypeVente is TypeVente.PouletsVivants
                                    or TypeVente.PouletsNettoyes
                                    or TypeVente.PouletsFumes
                ? (int)vente.Quantite
                : 0;
        }
    }
}
