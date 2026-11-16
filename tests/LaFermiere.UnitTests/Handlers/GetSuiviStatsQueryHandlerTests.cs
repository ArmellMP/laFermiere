using Application.Common.Exceptions;
using Application.Features.Bandes.Interfaces;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.SuivisJournaliers.Queries.GetSuiviStats;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Tests des statistiques du suivi journalier (bug corrigé le 2026-11-01, commit 3a505ff).
    ///
    /// Bug d'origine : erreur 500 quand il n'y avait aucun suivi (moyenne d'une liste vide),
    /// et les moyennes comptaient les valeurs absentes comme des 0.
    /// </summary>
    public class GetSuiviStatsQueryHandlerTests
    {
        private readonly ISuiviJournalierRepository _suiviRepository = Substitute.For<ISuiviJournalierRepository>();
        private readonly IBandeRepository _bandeRepository = Substitute.For<IBandeRepository>();
        private readonly GetSuiviStatsQueryHandler _handler;

        public GetSuiviStatsQueryHandlerTests()
        {
            _bandeRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Bande { Id = 1 });
            _handler = new GetSuiviStatsQueryHandler(
                _suiviRepository, _bandeRepository, NullLogger<GetSuiviStatsQueryHandler>.Instance);
        }

        // Configure ce que renvoie le faux repository, quels que soient les filtres demandés
        private void SuivisEnBase(params SuiviJournalier[] suivis) =>
            _suiviRepository
                .GetByFiltresAsync(Arg.Any<int?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
                .Returns(suivis);

        [Fact]
        public async Task AucunSuivi_RenvoieDesStatistiquesVidesSansErreur()
        {
            SuivisEnBase();   // liste vide

            var stats = await _handler.Handle(new GetSuiviStatsQuery { BandeId = 1 }, CancellationToken.None);

            Assert.Equal(0, stats.TotalSuivis);
            Assert.Equal(0, stats.TotalMortalites);
            Assert.Null(stats.PoidsMoyenGlobal);       // null = "pas de pesée", et non 0 kg
            Assert.Null(stats.TemperatureMoyenne);
        }

        [Fact]
        public async Task MoyennesCalculeesSurLesSeulesValeursPresentes()
        {
            // 3 suivis, dont un sans pesée ni température
            SuivisEnBase(
                new SuiviJournalier { Jour = 1, NbMortalites = 2, PoidsMoyen = 1.0m, Temperature = 30m },
                new SuiviJournalier { Jour = 2, NbMortalites = 1, PoidsMoyen = null, Temperature = null },
                new SuiviJournalier { Jour = 3, NbMortalites = 0, PoidsMoyen = 2.0m, Temperature = 26m });

            var stats = await _handler.Handle(new GetSuiviStatsQuery { BandeId = 1 }, CancellationToken.None);

            Assert.Equal(3, stats.TotalSuivis);
            Assert.Equal(3, stats.TotalMortalites);
            // (1,0 + 2,0) / 2 = 1,5 : le suivi sans pesée n'est PAS compté comme 0 kg
            // (sinon la moyenne serait faussée à 1,0)
            Assert.Equal(1.5m, stats.PoidsMoyenGlobal);
            Assert.Equal(28m, stats.TemperatureMoyenne);
        }

        [Fact]
        public async Task SansBandeId_StatistiquesGlobales()
        {
            SuivisEnBase(new SuiviJournalier { Jour = 1, NbMortalites = 4 });

            var stats = await _handler.Handle(new GetSuiviStatsQuery { BandeId = null }, CancellationToken.None);

            Assert.Equal(4, stats.TotalMortalites);
            // Sans bande demandée, on ne vérifie l'existence d'aucune bande
            await _bandeRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task BandeInexistante_LeveNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(new GetSuiviStatsQuery { BandeId = 999 }, CancellationToken.None));
        }
    }
}
