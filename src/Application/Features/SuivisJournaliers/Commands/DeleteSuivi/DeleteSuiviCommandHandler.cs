using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.Bandes.Interfaces;
using Application.Features.Stocks.Interfaces;

namespace Application.Features.SuivisJournaliers.Commands.DeleteSuivi
{
    /// <summary>
    /// Handler pour supprimer un suivi journalier. Gère également le rollback des impacts (mortalités, stocks).
    /// </summary>
    public class DeleteSuiviCommandHandler : IRequestHandler<DeleteSuiviCommand, Unit>
    {
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<DeleteSuiviCommandHandler> _logger;

        public DeleteSuiviCommandHandler(
            ISuiviJournalierRepository suiviRepository,
            IBandeRepository bandeRepository,
            IStockRepository stockRepository,
            ILogger<DeleteSuiviCommandHandler> logger)
        {
            _suiviRepository = suiviRepository;
            _bandeRepository = bandeRepository;
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteSuiviCommand request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression du suivi {SuiviId}", request.Id);

            // ============================================
            // 1. Récupérer le suivi
            // ============================================
            var suivi = await _suiviRepository.GetByIdAsync(request.Id, cancellationToken);
            if (suivi == null)
            {
                throw new NotFoundException($"Suivi avec l'ID {request.Id} non trouvé.");
            }

            // ============================================
            // 2. Restaurer la bande (annuler les mortalités)
            // ============================================
            if (suivi.NbMortalites > 0)
            {
                var bande = await _bandeRepository.GetByIdAsync(suivi.BandeId, cancellationToken);
                if (bande != null)
                {
                    bande.NbPoussinsActuel += suivi.NbMortalites;
                    bande.UpdatedAt = DateTime.UtcNow;
                    await _bandeRepository.UpdateAsync(bande, cancellationToken);
                    
                    _logger.LogInformation(
                        "Restauration de {Mortalites} poussins pour la bande {BandeId}",
                        suivi.NbMortalites, bande.Id);
                }
            }

            // ============================================
            // 3. Restaurer le stock (annuler la consommation)
            // ============================================
            if (suivi.StockConsommeId.HasValue && suivi.QuantiteConsommee.HasValue)
            {
                var stock = await _stockRepository.GetByIdAsync(suivi.StockConsommeId.Value, cancellationToken);
                if (stock != null)
                {
                    stock.QuantiteActuelle += suivi.QuantiteConsommee.Value;
                    stock.UpdatedAt = DateTime.UtcNow;
                    await _stockRepository.UpdateAsync(stock, cancellationToken);
                    
                    _logger.LogInformation(
                        "Restauration de {Quantite} {Unite} dans le stock {StockId}",
                        suivi.QuantiteConsommee, stock.UniteMesure, stock.Id);
                }
            }

            // ============================================
            // 4. Supprimer le suivi
            // ============================================
            await _suiviRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Suivi {SuiviId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}