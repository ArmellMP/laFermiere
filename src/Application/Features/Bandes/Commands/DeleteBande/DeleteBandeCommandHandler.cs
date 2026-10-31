using Application.Common.Exceptions;
using Application.Features.Bandes.Interfaces;
using MediatR;

namespace Application.Features.Bandes.Commands.DeleteBande
{
    /// <summary>
    /// Handler pour supprimer une bande.
    /// </summary>
    public class DeleteBandeCommandHandler 
        : IRequestHandler<DeleteBandeCommand, Unit>
    {
        private readonly IBandeRepository _bandeRepository;

        public DeleteBandeCommandHandler(IBandeRepository bandeRepository)
        {
            _bandeRepository = bandeRepository;
        }

        public async Task<Unit> Handle(
            DeleteBandeCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Vérifier que la bande existe
            var existingBande = await _bandeRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

            if (existingBande == null)
            {
                // Même convention que les autres entités : NotFoundException => HTTP 404.
                // (Avant : on retournait false, mais le contrôleur ignorait ce booléen
                // et répondait 204 "supprimé" pour une bande qui n'existait pas.)
                throw new NotFoundException($"Bande avec l'ID {request.Id} non trouvée.");
            }

            // 2. Valider qu'on peut supprimer cette bande
            ValidateCanDelete(existingBande);

            // 3. Supprimer la bande
            await _bandeRepository.DeleteAsync(request.Id, cancellationToken);

            // 4. Unit.Value = "terminé, rien à retourner"
            return Unit.Value;
        }
        
        /// <summary>
        /// Valide qu'on peut supprimer cette bande.
        /// </summary>
        private void ValidateCanDelete(Domain.Entities.Bande bande)
        {
            // Règle 1 : Ne pas supprimer une bande en cours
            if (bande.Statut == Domain.Enums.StatutBande.EnCours)
            {
                throw new BusinessRuleException(
                    "Impossible de supprimer une bande en cours d'élevage. " +
                    "Veuillez d'abord la clôturer.");
            }
            
            // Règle 2 : Vérifier s'il y a des données liées
            // TODO: Vérifier s'il y a des suivis journaliers, ventes, etc.
            // Pour l'instant, on suppose qu'il n'y en a pas
            // if (bande.HasRelatedData) ...
            
            // Règle 3 : Ne pas supprimer une bande archivée
            // (pour conserver l'historique)
            if (bande.Statut == Domain.Enums.StatutBande.Archivee)
            {
                throw new BusinessRuleException(
                    "Impossible de supprimer une bande archivée. " +
                    "L'historique doit être conservé.");
            }
        }
    }
}