using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Achats.Interfaces;
using Application.Features.Bandes.Interfaces;
using Domain.Enums;

namespace Application.Features.Achats.Commands.DeleteAchat
{
    public class DeleteAchatCommandHandler : IRequestHandler<DeleteAchatCommand, Unit>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly ILogger<DeleteAchatCommandHandler> _logger;

        public DeleteAchatCommandHandler(
            IAchatRepository achatRepository,
            IBandeRepository bandeRepository,
            ILogger<DeleteAchatCommandHandler> logger)
        {
            _achatRepository = achatRepository;
            _bandeRepository = bandeRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteAchatCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression de l'achat {AchatId}", request.Id);

            // 1. Récupérer l'achat
            var achat = await _achatRepository.GetByIdAsync(request.Id, cancellationToken);
            if (achat == null)
            {
                throw new NotFoundException($"Achat avec l'ID {request.Id} non trouvé.");
            }

            // 2. Restaurer la bande si c'était un achat de poussins
            if (achat.TypeAchat == TypeAchat.Poussins)
            {
                var bande = await _bandeRepository.GetByIdAsync(achat.BandeId, cancellationToken);
                if (bande != null)
                {
                    bande.NbPoussinsInit -= (int)achat.Quantite;
                    bande.NbPoussinsActuel -= (int)achat.Quantite;
                    bande.UpdatedAt = DateTime.UtcNow;
                    await _bandeRepository.UpdateAsync(bande, cancellationToken);
                    
                    _logger.LogInformation("Restauration de la bande {BandeId}: -{Quantite} poussins, nouveau total {NbTotal}",
                        bande.Id, achat.Quantite, bande.NbPoussinsActuel);
                }
            }

            // 3. Supprimer l'achat
            await _achatRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Achat {AchatId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}