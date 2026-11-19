using Application.Features.Alertes.Commands.GenererAlertes;
using Application.Features.Alertes.Interfaces;
using Application.Features.Bandes.Interfaces;
using Application.Features.Parametres.Interfaces;
using Application.Features.Stocks.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Règles de génération automatique des alertes (stock faible, péremption, bande à clôturer).
    /// Paramètres par défaut : seuil stock 10 %, péremption 15 jours, cycle 45 jours.
    /// </summary>
    public class GenererAlertesCommandHandlerTests
    {
        private readonly IAlerteRepository _alerteRepository = Substitute.For<IAlerteRepository>();
        private readonly IStockRepository _stockRepository = Substitute.For<IStockRepository>();
        private readonly IBandeRepository _bandeRepository = Substitute.For<IBandeRepository>();
        private readonly IParametreRepository _parametreRepository = Substitute.For<IParametreRepository>();
        private readonly GenererAlertesCommandHandler _handler;

        // Alertes enregistrées par le handler (capturées sur AddAsync)
        private readonly List<Alerte> _creees = new();

        private static readonly Bande BandeEnCours = new()
        {
            Id = 1, Code = "B-1", Statut = StatutBande.EnCours, DateArrivee = DateTime.Today.AddDays(-10)
        };

        public GenererAlertesCommandHandlerTests()
        {
            _bandeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { BandeEnCours });
            _stockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Stock>());
            _alerteRepository.GetNonLuesAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Alerte>());
            _alerteRepository.AddAsync(Arg.Do<Alerte>(a => _creees.Add(a)), Arg.Any<CancellationToken>())
                .Returns(ci => ci.Arg<Alerte>());
            // Aucun paramètre en base : valeurs par défaut
            _parametreRepository.GetValueAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);

            _handler = new GenererAlertesCommandHandler(_alerteRepository, _stockRepository, _bandeRepository,
                _parametreRepository, NullLogger<GenererAlertesCommandHandler>.Instance);
        }

        private void Stocks(params Stock[] stocks) =>
            _stockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(stocks);

        private static Stock Stock(int id, decimal initiale, decimal actuelle, decimal? seuil = null,
            DateTime? peremption = null, int bandeId = 1) => new()
        {
            Id = id, BandeId = bandeId, Nom = $"Produit {id}", Type = TypeStock.Aliment, UniteMesure = UniteMesure.Kg,
            QuantiteInitiale = initiale, QuantiteActuelle = actuelle, SeuilAlerte = seuil, DatePeremption = peremption
        };

        private Task<Application.Features.Alertes.Dtos.GenerationAlertesResultDto> Generer() =>
            _handler.Handle(new GenererAlertesCommand(), CancellationToken.None);

        // ============================================
        // STOCK FAIBLE
        // ============================================

        [Fact]
        public async Task StockSousLeSeuilEnPourcent_AlerteAvertissement()
        {
            Stocks(Stock(10, initiale: 100, actuelle: 8),     // 8 <= 10 % de 100 : alerte
                   Stock(11, initiale: 100, actuelle: 50));   // au-dessus : rien

            var resultat = await Generer();

            Assert.Equal(1, resultat.StockFaible);
            var alerte = Assert.Single(_creees);
            Assert.Equal((TypeAlerte.StockFaible, 10, NiveauAlerte.Avertissement), (alerte.TypeAlerte, alerte.EntiteId, alerte.Niveau));
            Assert.Contains("Produit 10", alerte.Message);
        }

        [Fact]
        public async Task SeuilPropreAuStock_PrioritaireSurLePourcentage()
        {
            // 30 > 10 % de 100, mais le stock a son propre seuil de 40
            Stocks(Stock(10, initiale: 100, actuelle: 30, seuil: 40));

            Assert.Equal(1, (await Generer()).StockFaible);
        }

        [Fact]
        public async Task StockEpuise_AlerteCritique()
        {
            Stocks(Stock(10, initiale: 100, actuelle: 0));

            await Generer();

            Assert.Equal(NiveauAlerte.Critique, Assert.Single(_creees).Niveau);
        }

        [Fact]
        public async Task PourcentageLuDansLesParametres()
        {
            _parametreRepository.GetValueAsync(GenererAlertesCommandHandler.CleSeuilStock, Arg.Any<CancellationToken>())
                .Returns("30");
            Stocks(Stock(10, initiale: 100, actuelle: 25));   // 25 <= 30 % : alerte (pas avec 10 %)

            Assert.Equal(1, (await Generer()).StockFaible);
        }

        // ============================================
        // PÉREMPTION
        // ============================================

        [Fact]
        public async Task Peremption_ProcheAvertissement_DepasseeCritique_LointaineRien()
        {
            Stocks(Stock(10, 100, 50, peremption: DateTime.Today.AddDays(5)),     // proche
                   Stock(11, 100, 50, peremption: DateTime.Today.AddDays(-2)),    // dépassée
                   Stock(12, 100, 50, peremption: DateTime.Today.AddDays(60)));   // lointaine

            var resultat = await Generer();

            Assert.Equal(2, resultat.Peremption);
            Assert.Equal(NiveauAlerte.Avertissement, _creees.Single(a => a.EntiteId == 10).Niveau);
            Assert.Equal(NiveauAlerte.Critique, _creees.Single(a => a.EntiteId == 11).Niveau);
        }

        [Fact]
        public async Task StockVidePerime_PasDAlertePeremption()
        {
            Stocks(Stock(10, 100, 0, peremption: DateTime.Today.AddDays(-2)));

            var resultat = await Generer();

            Assert.Equal(0, resultat.Peremption);   // rien à retirer ; seule la rupture est signalée
            Assert.Equal(1, resultat.StockFaible);
        }

        // ============================================
        // BANDE À CLÔTURER
        // ============================================

        [Fact]
        public async Task BandeEnCoursAuDelaDuCycle_AlerteBandeACloturer()
        {
            var ancienne = new Bande { Id = 2, Code = "B-2", Statut = StatutBande.EnCours, DateArrivee = DateTime.Today.AddDays(-46) };
            var enPreparation = new Bande { Id = 3, Code = "B-3", Statut = StatutBande.EnPreparation, DateArrivee = DateTime.Today.AddDays(-60) };
            _bandeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { BandeEnCours, ancienne, enPreparation });

            var resultat = await Generer();

            Assert.Equal(1, resultat.BandeACloturer);
            Assert.Equal(2, Assert.Single(_creees).EntiteId);   // ni la bande récente, ni celle en préparation
        }

        // ============================================
        // RÈGLES COMMUNES
        // ============================================

        [Fact]
        public async Task StocksDUneBandeCloturee_Ignores()
        {
            var cloturee = new Bande { Id = 5, Code = "B-5", Statut = StatutBande.Cloturee, DateArrivee = DateTime.Today.AddDays(-50) };
            _bandeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { BandeEnCours, cloturee });
            Stocks(Stock(10, 100, 0, bandeId: 5));

            Assert.Equal(0, (await Generer()).Total);
        }

        [Fact]
        public async Task AlerteNonLueDejaPresente_PasDeDoublon()
        {
            Stocks(Stock(10, initiale: 100, actuelle: 5));
            _alerteRepository.GetNonLuesAsync(Arg.Any<CancellationToken>()).Returns(new[]
            {
                new Alerte { TypeAlerte = TypeAlerte.StockFaible, EntiteId = 10, EstLue = false }
            });

            var resultat = await Generer();

            Assert.Equal(0, resultat.Total);
            Assert.Empty(_creees);
        }
    }
}
