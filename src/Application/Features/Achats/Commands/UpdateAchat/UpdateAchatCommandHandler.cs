using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Application.Features.Bandes.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Achats.Commands.UpdateAchat
{
    public class UpdateAchatCommandHandler : IRequestHandler<UpdateAchatCommand, AchatResponseDto>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly ILogger<UpdateAchatCommandHandler> _logger;

        public UpdateAchatCommandHandler(
            IAchatRepository achatRepository,
            IBandeRepository bandeRepository,
            ILogger<UpdateAchatCommandHandler> logger)
        {
            _achatRepository = achatRepository;
            _bandeRepository = bandeRepository;
            _logger = logger;
        }

        public async Task<AchatResponseDto> Handle(
            UpdateAchatCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateAchatDto;

            _logger.LogInformation("Mise à jour de l'achat {AchatId}", updateDto.Id);

            // 1. Récupérer l'achat existant
            var existingAchat = await _achatRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingAchat == null)
            {
                throw new NotFoundException($"Achat avec l'ID {updateDto.Id} non trouvé.");
            }

            // Nombre de poussins que cet achat ajoutait à la bande AVANT modification
            // (à retenir maintenant : les propriétés vont être modifiées juste après)
            var poussinsAjoutesAvant = PoussinsAjoutesALaBande(existingAchat);

            // 2. Mise à jour des propriétés (un champ null = "ne pas modifier")
            if (updateDto.DateAchat.HasValue)
                existingAchat.DateAchat = updateDto.DateAchat.Value;

            if (!string.IsNullOrEmpty(updateDto.TypeAchat))
            {
                if (Enum.TryParse<TypeAchat>(updateDto.TypeAchat, true, out var typeAchat))
                {
                    existingAchat.TypeAchat = typeAchat;
                }
            }

            existingAchat.Description = updateDto.Description ?? existingAchat.Description;

            if (updateDto.Quantite.HasValue)
                existingAchat.Quantite = updateDto.Quantite.Value;

            if (!string.IsNullOrEmpty(updateDto.UniteMesure))
            {
                if (Enum.TryParse<UniteMesureAchat>(updateDto.UniteMesure, true, out var uniteMesure))
                {
                    existingAchat.UniteMesure = uniteMesure;
                }
            }

            if (updateDto.PrixUnitaire.HasValue)
                existingAchat.PrixUnitaire = updateDto.PrixUnitaire.Value;

            // Le montant total est TOUJOURS recalculé, quel que soit le champ modifié.
            // (Avant : seulement si le prix était envoyé. Modifier la quantité seule laissait
            //  l'ancien montant, et la contrainte SQL chk_montant_achat
            //  "MontantTotal = Quantite * PrixUnitaire" refusait la mise à jour => erreur 500.)
            existingAchat.MontantTotal = existingAchat.Quantite * existingAchat.PrixUnitaire;

            existingAchat.FactureNumero = updateDto.FactureNumero ?? existingAchat.FactureNumero;
            existingAchat.DateReception = updateDto.DateReception ?? existingAchat.DateReception;
            existingAchat.Observations = updateDto.Observations ?? existingAchat.Observations;

            // La réception ne peut pas précéder l'achat (contrainte SQL chk_date_reception).
            // Le validateur ne peut pas le vérifier si une seule des deux dates est envoyée :
            // il ne connaît pas la valeur enregistrée en base. Ici, on a les deux dates
            // définitives => refus clair (400) au lieu d'une erreur SQL (500).
            if (existingAchat.DateReception.HasValue && existingAchat.DateReception.Value < existingAchat.DateAchat)
            {
                throw new BusinessRuleException(
                    $"La date de réception ({existingAchat.DateReception.Value:dd/MM/yyyy}) ne peut pas " +
                    $"précéder la date d'achat ({existingAchat.DateAchat:dd/MM/yyyy}).");
            }

            // 3. Impact sur l'effectif de la bande (achats de poussins uniquement)
            // La création ajoute les poussins à la bande, la suppression les retire.
            // La modification doit donc appliquer la DIFFÉRENCE, sinon le compteur se décale.
            // (Le STOCK, lui, est ajusté par le trigger SQL trg_after_achat_update.)
            var difference = PoussinsAjoutesALaBande(existingAchat) - poussinsAjoutesAvant;   // < 0 : on retire des poussins

            Bande? bande = null;
            if (difference != 0)
            {
                bande = await _bandeRepository.GetByIdAsync(existingAchat.BandeId, cancellationToken)
                    ?? throw new NotFoundException($"Bande avec l'ID {existingAchat.BandeId} non trouvée.");

                // Vérifier AVANT d'écrire quoi que ce soit (mêmes règles que les contraintes SQL
                // chk_nb_poussins_actuel >= 0 et chk_nb_poussins_init > 0) :
                // on ne retire pas des poussins déjà morts ou vendus.
                if (difference < 0 &&
                    (bande.NbPoussinsActuel + difference < 0 || bande.NbPoussinsInit + difference <= 0))
                {
                    throw new BusinessRuleException(
                        $"Effectif insuffisant. Poussins présents dans la bande : {bande.NbPoussinsActuel}, " +
                        $"poussins à retirer : {-difference}");
                }

                // Init et Actuel bougent ensemble : morts et vendus (Init - Actuel) ne changent pas
                bande.NbPoussinsInit += difference;
                bande.NbPoussinsActuel += difference;
                bande.UpdatedAt = DateTime.UtcNow;
            }

            // 4. Sauvegarde : l'achat d'abord (le trigger peut refuser => rien n'est écrit),
            //    puis la bande si son effectif a changé
            await _achatRepository.UpdateAsync(existingAchat, cancellationToken);

            if (bande != null)
            {
                await _bandeRepository.UpdateAsync(bande, cancellationToken);
                _logger.LogInformation(
                    "Bande {BandeId} : effectif ajusté de {Difference} suite à la modification de l'achat {AchatId}",
                    bande.Id, difference, existingAchat.Id);
            }

            _logger.LogInformation("Achat {AchatId} mis à jour avec succès", updateDto.Id);

            // 5. Retour de la réponse
            return existingAchat.ToResponseDto();
        }

        /// <summary>
        /// Nombre de poussins qu'un achat ajoute à l'effectif de sa bande.
        /// Même règle que la création et la suppression : seuls les achats de type
        /// Poussins comptent ; les autres types vont en stock (ou nulle part) = 0.
        /// </summary>
        private static int PoussinsAjoutesALaBande(Achat achat)
        {
            return achat.TypeAchat == TypeAchat.Poussins ? (int)achat.Quantite : 0;
        }
    }
}