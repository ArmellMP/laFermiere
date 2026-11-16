using Application.Features.Alertes.Dtos;
using Application.Features.Alertes.Interfaces;
using Application.Features.Bandes.Interfaces;
using Application.Features.Parametres.Interfaces;
using Application.Features.Stocks.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Alertes.Commands.GenererAlertes
{
    /// <summary>
    /// Génère les alertes métier à partir de l'état des stocks et des bandes.
    ///
    /// Seuils lus dans la table Parametres (valeur par défaut si la clé est absente) :
    ///   SeuilAlerteStock      (%)    : seuil d'un stock sans SeuilAlerte propre, en % de sa quantité initiale
    ///   JoursAlertePeremption (jours): délai avant péremption qui déclenche l'alerte
    ///   DureeCycle            (jours): âge à partir duquel une bande en cours est "à clôturer"
    ///
    /// Seules les bandes actives (en préparation ou en cours) et leurs stocks sont surveillés :
    /// les restes d'une bande clôturée sont notés à sa clôture, pas signalés en continu.
    ///
    /// Pas de doublon : si une alerte NON LUE existe déjà pour le même type et la même entité,
    /// la situation est déjà signalée et rien n'est créé. Une fois l'alerte lue, une nouvelle
    /// génération la recrée si le problème persiste.
    /// </summary>
    public class GenererAlertesCommandHandler
        : IRequestHandler<GenererAlertesCommand, GenerationAlertesResultDto>
    {
        public const string CleSeuilStock = "SeuilAlerteStock";
        public const string CleJoursPeremption = "JoursAlertePeremption";
        public const string CleDureeCycle = "DureeCycle";

        public const int SeuilStockParDefaut = 10;
        public const int JoursPeremptionParDefaut = 15;
        public const int DureeCycleParDefaut = 45;

        private readonly IAlerteRepository _alerteRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IParametreRepository _parametreRepository;
        private readonly ILogger<GenererAlertesCommandHandler> _logger;

        public GenererAlertesCommandHandler(
            IAlerteRepository alerteRepository,
            IStockRepository stockRepository,
            IBandeRepository bandeRepository,
            IParametreRepository parametreRepository,
            ILogger<GenererAlertesCommandHandler> logger)
        {
            _alerteRepository = alerteRepository;
            _stockRepository = stockRepository;
            _bandeRepository = bandeRepository;
            _parametreRepository = parametreRepository;
            _logger = logger;
        }

        public async Task<GenerationAlertesResultDto> Handle(
            GenererAlertesCommand request,
            CancellationToken cancellationToken)
        {
            var aujourdHui = DateTime.Today;

            // 1. Paramètres
            var seuilStockPourcent = await LireEntierAsync(CleSeuilStock, SeuilStockParDefaut, cancellationToken);
            var joursPeremption = await LireEntierAsync(CleJoursPeremption, JoursPeremptionParDefaut, cancellationToken);
            var dureeCycle = await LireEntierAsync(CleDureeCycle, DureeCycleParDefaut, cancellationToken);

            // 2. Bandes actives et leurs stocks
            var bandesActives = (await _bandeRepository.GetAllAsync(cancellationToken))
                .Where(b => b.Statut is StatutBande.EnPreparation or StatutBande.EnCours)
                .ToDictionary(b => b.Id);

            var stocksActifs = (await _stockRepository.GetAllAsync(cancellationToken))
                .Where(s => bandesActives.ContainsKey(s.BandeId))
                .ToList();

            // 3. Situations déjà signalées (alertes non lues)
            var dejaSignalees = (await _alerteRepository.GetNonLuesAsync(cancellationToken))
                .Select(a => (a.TypeAlerte, a.EntiteId))
                .ToHashSet();

            var nouvelles = new List<Alerte>();

            // 4. Stock faible
            foreach (var stock in stocksActifs)
            {
                var seuil = stock.SeuilAlerte ?? stock.QuantiteInitiale * seuilStockPourcent / 100m;
                if (stock.QuantiteInitiale <= 0 || stock.QuantiteActuelle > seuil)
                    continue;

                var code = bandesActives[stock.BandeId].Code;
                nouvelles.Add(Creer(TypeAlerte.StockFaible, stock.Id,
                    stock.QuantiteActuelle == 0 ? NiveauAlerte.Critique : NiveauAlerte.Avertissement,
                    stock.QuantiteActuelle == 0
                        ? $"Rupture de stock : {stock.Nom} (bande {code}) est épuisé."
                        : $"Stock faible : {stock.Nom} (bande {code}), reste {stock.QuantiteActuelle:0.##} {stock.UniteMesure} " +
                          $"(seuil {seuil:0.##} {stock.UniteMesure})."));
            }

            // 5. Péremption (seulement s'il reste quelque chose à périmer)
            foreach (var stock in stocksActifs.Where(s => s.DatePeremption.HasValue && s.QuantiteActuelle > 0))
            {
                var joursRestants = (stock.DatePeremption!.Value.Date - aujourdHui).Days;
                if (joursRestants > joursPeremption)
                    continue;

                var code = bandesActives[stock.BandeId].Code;
                nouvelles.Add(Creer(TypeAlerte.Peremption, stock.Id,
                    joursRestants < 0 ? NiveauAlerte.Critique : NiveauAlerte.Avertissement,
                    joursRestants < 0
                        ? $"Produit périmé depuis le {stock.DatePeremption:dd/MM/yyyy} : {stock.Nom} (bande {code}), " +
                          $"{stock.QuantiteActuelle:0.##} {stock.UniteMesure} à retirer."
                        : $"Péremption proche : {stock.Nom} (bande {code}) périme le {stock.DatePeremption:dd/MM/yyyy} " +
                          $"(dans {joursRestants} jour(s))."));
            }

            // 6. Bandes à clôturer
            foreach (var bande in bandesActives.Values.Where(b => b.Statut == StatutBande.EnCours))
            {
                var age = (aujourdHui - bande.DateArrivee.Date).Days;
                if (age < dureeCycle)
                    continue;

                nouvelles.Add(Creer(TypeAlerte.BandeACloturer, bande.Id, NiveauAlerte.Avertissement,
                    $"Bande {bande.Code} : {age} jours d'élevage (durée normale {dureeCycle} jours). " +
                    "Pensez à la clôturer."));
            }

            // 7. Enregistrer ce qui n'est pas déjà signalé
            var resultat = new GenerationAlertesResultDto();
            foreach (var alerte in nouvelles.Where(a => !dejaSignalees.Contains((a.TypeAlerte, a.EntiteId))))
            {
                await _alerteRepository.AddAsync(alerte, cancellationToken);

                switch (alerte.TypeAlerte)
                {
                    case TypeAlerte.StockFaible: resultat.StockFaible++; break;
                    case TypeAlerte.Peremption: resultat.Peremption++; break;
                    case TypeAlerte.BandeACloturer: resultat.BandeACloturer++; break;
                }
            }

            _logger.LogInformation(
                "Génération des alertes : {Total} créée(s) (stock faible {StockFaible}, péremption {Peremption}, bande à clôturer {BandeACloturer})",
                resultat.Total, resultat.StockFaible, resultat.Peremption, resultat.BandeACloturer);

            return resultat;
        }

        private static Alerte Creer(TypeAlerte type, int entiteId, NiveauAlerte niveau, string message) => new()
        {
            TypeAlerte = type,
            EntiteId = entiteId,
            Niveau = niveau,
            Message = message,
            EstLue = false,
            DateAlerte = DateTime.Now
        };

        /// <summary>
        /// Lit un paramètre entier ; valeur par défaut si la clé est absente ou illisible.
        /// </summary>
        private async Task<int> LireEntierAsync(string cle, int defaut, CancellationToken cancellationToken)
        {
            var valeur = await _parametreRepository.GetValueAsync(cle, cancellationToken);
            if (int.TryParse(valeur, out var entier) && entier >= 0)
                return entier;

            if (valeur != null)
                _logger.LogWarning("Paramètre {Cle} illisible ('{Valeur}') : valeur par défaut {Defaut} utilisée", cle, valeur, defaut);

            return defaut;
        }
    }
}
