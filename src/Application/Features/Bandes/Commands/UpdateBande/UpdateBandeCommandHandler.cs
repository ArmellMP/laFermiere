using Application.Common.Exceptions;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using Domain.Entities;
using Domain.Enums; 
using MediatR;
using Application.Common.Mappings;

namespace Application.Features.Bandes.Commands.UpdateBande
{
    /// <summary>
    /// Handler pour mettre à jour une bande existante.
    /// </summary>
    public class UpdateBandeCommandHandler 
        : IRequestHandler<UpdateBandeCommand, BandeResponseDto>
    {
        private readonly IBandeRepository _bandeRepository;

        public UpdateBandeCommandHandler(
            IBandeRepository bandeRepository)
        {
            _bandeRepository = bandeRepository;
        }

        public async Task<BandeResponseDto> Handle(
            UpdateBandeCommand request, 
            CancellationToken cancellationToken)
        {
            // ✅ CORRECTION 1 : Utiliser UpdateBandeDto
            var updateDto = request.UpdateBandeDto;
            
            // 1. Vérifier que la bande existe
            var existingBande = await GetExistingBandeAsync(updateDto.Id, cancellationToken);
            
            // 2. Appliquer les modifications depuis le DTO
            UpdateBandeFromDto(existingBande, updateDto);
            
            // 3. Valider les règles métier après mise à jour
            ValidateBusinessRulesForUpdate(existingBande);
            
            // 4. Mettre à jour le timestamp
            existingBande.UpdatedAt = DateTime.Now;
            
            // 5. Sauvegarder les changements
            await _bandeRepository.UpdateAsync(existingBande, cancellationToken);
            
            // 6. Convertir en DTO pour la réponse
            var bandeDto = existingBande.ToResponseDto();
            
            return bandeDto;
        }
        
        /// <summary>
        /// Récupère la bande existante ou lance une exception si non trouvée.
        /// </summary>
        private async Task<Bande> GetExistingBandeAsync(
            int bandeId, 
            CancellationToken cancellationToken)
        {
            var bande = await _bandeRepository.GetByIdAsync(bandeId, cancellationToken);
            
            if (bande == null)
            {
                throw new NotFoundException($"Bande avec l'ID {bandeId} non trouvée.");
            }
            
            return bande;
        }
        
        /// <summary>
        /// Applique les modifications depuis le DTO vers l'entité existante.
        /// </summary>
        private void UpdateBandeFromDto(Bande bande, UpdateBandeDto dto)
        {
            // ✅ CORRECTION 2 : Mise à jour avec UpdateBandeDto
            bande.NbPoussinsActuel = dto.NbPoussinsActuel;
            bande.Observations = dto.Observations ?? bande.Observations;
            bande.Statut = dto.Statut;
            bande.DateCloture = dto.DateCloture;
            
            // ✅ REMARQUE : Le Code n'est PAS modifiable en Update !
            // Si vous voulez permettre la modification du code, ajoutez-le dans UpdateBandeDto
        }
        
        /// <summary>
        /// Valide les règles métier spécifiques à la mise à jour.
        /// </summary>
        private void ValidateBusinessRulesForUpdate(Bande bande)
        {
            // Règle 1 : NbPoussinsActuel ne peut être négatif
            if (bande.NbPoussinsActuel < 0)
            {
                throw new BusinessRuleException(
                    "Le nombre actuel de poussins ne peut être négatif.");
            }
            
            // Règle 2 : NbPoussinsActuel ne peut dépasser NbPoussinsInit
            if (bande.NbPoussinsActuel > bande.NbPoussinsInit)
            {
                throw new BusinessRuleException(
                    $"Le nombre actuel de poussins ({bande.NbPoussinsActuel}) " +
                    $"ne peut dépasser le nombre initial ({bande.NbPoussinsInit}).");
            }
            
            // Règle 3 : Vérifier la cohérence des dates si bande clôturée
            if (bande.Statut == StatutBande.Cloturee && !bande.DateCloture.HasValue)
            {
                throw new BusinessRuleException(
                    "Une bande clôturée doit avoir une date de clôture.");
            }
            
            // Règle 4 : Ne pas modifier une bande archivée
            if (bande.Statut == StatutBande.Archivee)
            {
                throw new BusinessRuleException(
                    "Impossible de modifier une bande archivée.");
            }
        }
    }
}