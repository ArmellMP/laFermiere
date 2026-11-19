using Application.Common.Exceptions;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using Application.Features.Stocks.Interfaces;
using Application.Features.SuivisJournaliers.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Application.Common.Mappings;

namespace Application.Features.Bandes.Commands.CloturerBande
{
    /// <summary>
    /// Handler pour clôturer une bande.
    /// Opération métier importante avec beaucoup de validations.
    /// </summary>
    public class CloturerBandeCommandHandler
        : IRequestHandler<CloturerBandeCommand, BandeResponseDto>
    {
        /// <summary>
        /// Dernier jour qu'un suivi peut porter (contrainte chk_jour_range de SuivisJournaliers).
        /// </summary>
        private const int JourMaxSuivi = 45;

        private readonly IBandeRepository _bandeRepository;
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IStockRepository _stockRepository;

        public CloturerBandeCommandHandler(
            IBandeRepository bandeRepository,
            ISuiviJournalierRepository suiviRepository,
            IStockRepository stockRepository)
        {
            _bandeRepository = bandeRepository;
            _suiviRepository = suiviRepository;
            _stockRepository = stockRepository;
        }

        public async Task<BandeResponseDto> Handle(
            CloturerBandeCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Récupérer la bande
            var bande = await GetBandeAsync(request.Id, cancellationToken);

            // La date de clôture sert à toutes les vérifications (durée du cycle,
            // suivis attendus) : une clôture saisie après coup reste cohérente.
            var dateCloture = (request.DateCloture ?? DateTime.Now).Date;

            // 2. Valider qu'on peut clôturer cette bande
            ValidateCanCloture(bande, dateCloture);
            await ValidateSuivisCompletsAsync(bande, dateCloture, cancellationToken);

            // 3. Relever les stocks restants (avertissement, pas un blocage)
            var stocksRestants = await GetStocksRestantsAsync(bande.Id, cancellationToken);

            // 4. Appliquer la clôture
            ApplyCloture(bande, dateCloture, request.ObservationsCloture, stocksRestants);

            // 5. Mettre à jour le timestamp
            bande.UpdatedAt = DateTime.Now;

            // 6. Sauvegarder
            await _bandeRepository.UpdateAsync(bande, cancellationToken);

            // 7. Convertir en DTO pour la réponse
            var bandeDto = bande.ToResponseDto();
            bandeDto.IncludeDetails = true; // Toujours inclure les détails après clôture

            return bandeDto;
        }

        /// <summary>
        /// Récupère une bande ou lance NotFoundException.
        /// </summary>
        private async Task<Bande> GetBandeAsync(
            int bandeId,
            CancellationToken cancellationToken)
        {
            var bande = await _bandeRepository.GetByIdAsync(bandeId, cancellationToken);

            if (bande == null)
            {
                throw new NotFoundException(
                    $"Bande avec l'ID {bandeId} non trouvée.");
            }

            return bande;
        }

        /// <summary>
        /// Valide qu'on peut clôturer cette bande (règles sur la bande elle-même).
        /// </summary>
        private void ValidateCanCloture(Bande bande, DateTime dateCloture)
        {
            // Règle 1 : Ne peut clôturer que les bandes "EnCours"
            if (bande.Statut != StatutBande.EnCours)
            {
                var statutLibelle = GetStatutLibelle(bande.Statut);
                throw new BusinessRuleException(
                    $"Impossible de clôturer une bande avec le statut '{statutLibelle}'. " +
                    "Seules les bandes 'En cours' peuvent être clôturées.");
            }

            // Règle 2 : Vérifier le nombre de poussins
            if (bande.NbPoussinsActuel <= 0)
            {
                throw new BusinessRuleException(
                    "Impossible de clôturer une bande sans poussins.");
            }

            // Règle 3 : La clôture ne peut précéder l'arrivée
            if (dateCloture < bande.DateArrivee.Date)
            {
                throw new BusinessRuleException(
                    "La date de clôture ne peut être antérieure à la date d'arrivée.");
            }

            // Règle 4 : Vérifier la date d'arrivée (au moins 1 jour)
            var joursEcoules = (dateCloture - bande.DateArrivee.Date).Days;
            if (joursEcoules < 1)
            {
                throw new BusinessRuleException(
                    "Impossible de clôturer une bande le jour de son arrivée.");
            }

            // Règle 5 : Vérifier si le cycle est trop court (< 30 jours)
            // (cycle normal = 45 jours, mais minimum 30)
            if (joursEcoules < 30)
            {
                throw new BusinessRuleException(
                    $"Cycle trop court ({joursEcoules} jours). " +
                    "Le cycle minimum est de 30 jours.");
            }

            // Règle 6 : Vérifier le taux de mortalité (maximum 20%)
            var tauxMortalite = CalculateTauxMortalite(bande);
            if (tauxMortalite > 20)
            {
                throw new BusinessRuleException(
                    $"Taux de mortalité trop élevé ({tauxMortalite:F1}%). " +
                    "Le maximum autorisé est de 20%.");
            }
        }

        /// <summary>
        /// Règle 7 : chaque jour d'élevage avant la clôture doit avoir son suivi.
        /// Jour 1 = jour d'arrivée ; le jour de la clôture lui-même n'est pas exigé.
        /// Au-delà du jour 45, la base n'accepte plus de suivi : on n'exige donc rien après.
        /// </summary>
        private async Task ValidateSuivisCompletsAsync(
            Bande bande,
            DateTime dateCloture,
            CancellationToken cancellationToken)
        {
            var dernierJourAttendu = Math.Min(
                (dateCloture - bande.DateArrivee.Date).Days,
                JourMaxSuivi);

            var suivis = await _suiviRepository.GetByBandeIdAsync(bande.Id, cancellationToken);
            var joursSaisis = suivis.Select(s => s.Jour).ToHashSet();

            var joursManquants = Enumerable.Range(1, dernierJourAttendu)
                .Where(jour => !joursSaisis.Contains(jour))
                .ToList();

            if (joursManquants.Count > 0)
            {
                throw new BusinessRuleException(
                    $"Impossible de clôturer : {joursManquants.Count} suivi(s) journalier(s) manquant(s) " +
                    $"(jour(s) {FormatPlagesJours(joursManquants)}). " +
                    "Saisissez-les avant de clôturer la bande.");
            }
        }

        /// <summary>
        /// Stocks de la bande qui ne sont pas entièrement consommés.
        /// </summary>
        private async Task<List<Stock>> GetStocksRestantsAsync(
            int bandeId,
            CancellationToken cancellationToken)
        {
            var stocks = await _stockRepository.GetByBandeIdAsync(bandeId, cancellationToken);
            return stocks.Where(s => s.QuantiteActuelle > 0).ToList();
        }

        /// <summary>
        /// Applique la clôture à la bande.
        /// </summary>
        private void ApplyCloture(
            Bande bande,
            DateTime dateCloture,
            string? observationsCloture,
            List<Stock> stocksRestants)
        {
            // 1. Changer le statut et dater la clôture
            bande.Statut = StatutBande.Cloturee;
            bande.DateCloture = dateCloture;

            // 2. Bloc "CLÔTURE" ajouté aux observations : observations saisies
            //    et stocks restants (règle 8 : avertissement, la clôture n'est pas bloquée)
            var lignes = new List<string>();

            if (!string.IsNullOrWhiteSpace(observationsCloture))
                lignes.Add($"Observations: {observationsCloture}");

            if (stocksRestants.Count > 0)
            {
                lignes.Add("Stocks non consommés: " + string.Join(", ",
                    stocksRestants.Select(s => $"{s.Nom} {s.QuantiteActuelle:0.##} {s.UniteMesure}")));
            }

            if (lignes.Count == 0)
                return;

            var observationsExistantes = bande.Observations ?? string.Empty;
            var separateur = string.IsNullOrEmpty(observationsExistantes) ? "" : "\n\n";
            bande.Observations = observationsExistantes + separateur +
                                 "=== CLÔTURE ===\n" +
                                 $"Date: {dateCloture:dd/MM/yyyy}\n" +
                                 string.Join("\n", lignes);
        }

        /// <summary>
        /// Regroupe des jours consécutifs : [3,4,5,9] => "3-5, 9".
        /// </summary>
        private static string FormatPlagesJours(List<int> jours)
        {
            var plages = new List<string>();
            var debut = jours[0];
            var precedent = jours[0];

            foreach (var jour in jours.Skip(1).Append(int.MinValue))
            {
                if (jour == precedent + 1)
                {
                    precedent = jour;
                    continue;
                }

                plages.Add(debut == precedent ? $"{debut}" : $"{debut}-{precedent}");
                debut = precedent = jour;
            }

            return string.Join(", ", plages);
        }

        /// <summary>
        /// Calcule le taux de mortalité.
        /// </summary>
        private decimal CalculateTauxMortalite(Bande bande)
        {
            if (bande.NbPoussinsInit <= 0)
                return 0;

            var mortalites = bande.NbPoussinsInit - bande.NbPoussinsActuel;
            return (mortalites / (decimal)bande.NbPoussinsInit) * 100;
        }

        /// <summary>
        /// Convertit un statut en libellé lisible.
        /// </summary>
        private string GetStatutLibelle(StatutBande statut)
        {
            return statut switch
            {
                StatutBande.EnPreparation => "En préparation",
                StatutBande.EnCours => "En cours",
                StatutBande.Cloturee => "Clôturée",
                StatutBande.Archivee => "Archivée",
                _ => "Inconnu"
            };
        }
    }
}
