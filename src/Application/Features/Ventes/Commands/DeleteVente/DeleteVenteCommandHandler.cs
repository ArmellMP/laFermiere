using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Ventes.Interfaces;
using Application.Features.Bandes.Interfaces;
using Domain.Enums;  

namespace Application.Features.Ventes.Commands.DeleteVente
{
    public class DeleteVenteCommandHandler : IRequestHandler<DeleteVenteCommand, Unit>
    {
        private readonly IVenteRepository _venteRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly ILogger<DeleteVenteCommandHandler> _logger;

        public DeleteVenteCommandHandler(
            IVenteRepository venteRepository,
            IBandeRepository bandeRepository,
            ILogger<DeleteVenteCommandHandler> logger)
        {
            _venteRepository = venteRepository;
            _bandeRepository = bandeRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteVenteCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression de la vente {VenteId}", request.Id);

            // 1. Récupérer la vente
            var vente = await _venteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (vente == null)
            {
                throw new NotFoundException($"Vente avec l'ID {request.Id} non trouvée.");
            }

            // 2. Restaurer la bande (si c'est une vente de poussins)
            // ✅ CORRECTION : Utiliser Domain.Enums.TypeVente
            if (vente.TypeVente == Domain.Enums.TypeVente.PouletsVivants || 
                vente.TypeVente == Domain.Enums.TypeVente.PouletsNettoyes || 
                vente.TypeVente == Domain.Enums.TypeVente.PouletsFumes)
            {
                var bande = await _bandeRepository.GetByIdAsync(vente.BandeId, cancellationToken);
                if (bande != null)
                {
                    bande.NbPoussinsActuel += (int)vente.Quantite;
                    bande.UpdatedAt = DateTime.UtcNow;
                    await _bandeRepository.UpdateAsync(bande, cancellationToken);
                    
                    _logger.LogInformation("Restauration de {Quantite} poussins pour la bande {BandeId}",
                        vente.Quantite, bande.Id);
                }
            }

            // 3. Supprimer la vente
            await _venteRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Vente {VenteId} supprimée avec succès", request.Id);

            return Unit.Value;
        }
    }
}