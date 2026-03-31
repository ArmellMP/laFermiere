using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Fournisseurs.Interfaces;

namespace Application.Features.Fournisseurs.Commands.DeleteFournisseur
{
    public class DeleteFournisseurCommandHandler : IRequestHandler<DeleteFournisseurCommand, Unit>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly ILogger<DeleteFournisseurCommandHandler> _logger;

        public DeleteFournisseurCommandHandler(
            IFournisseurRepository fournisseurRepository,
            ILogger<DeleteFournisseurCommandHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteFournisseurCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression du fournisseur {FournisseurId}", request.Id);

            // 1. Vérifier que le fournisseur existe
            var exists = await _fournisseurRepository.ExistsAsync(request.Id, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException($"Fournisseur avec l'ID {request.Id} non trouvé.");
            }

            // 2. Vérifier que le fournisseur n'a pas d'achats
            var hasAchats = await _fournisseurRepository.HasAchatsAsync(request.Id, cancellationToken);
            if (hasAchats)
            {
                throw new BusinessRuleException(
                    "Impossible de supprimer un fournisseur qui a déjà des achats. " +
                    "Vous pouvez le désactiver à la place.");
            }

            // 3. Supprimer le fournisseur
            await _fournisseurRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Fournisseur {FournisseurId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}