using Application.Common.Exceptions;
using Application.Features.Achats.Commands.DeleteAchat;
using Application.Features.Achats.Commands.UpdateAchat;
using Application.Features.Achats.Dtos;
using Application.Features.Achats.Interfaces;
using Application.Features.Bandes.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Tests de la modification et de la suppression d'un achat (commits 132b16c, c7927fb).
    ///
    /// Règles protégées :
    ///   - montant toujours recalculé (sinon erreur SQL chk_montant_achat)
    ///   - achat de POUSSINS modifié : la différence est appliquée à la bande (Init et Actuel)
    ///   - refus si les poussins à retirer sont déjà morts ou vendus, sans rien écrire
    ///   - date de réception jamais avant la date d'achat, même si une seule date est envoyée
    ///   - suppression d'un achat de poussins : ils sont retirés de la bande
    ///
    /// Rappel : le STOCK (achats d'aliment, de médicaments...) est géré par les triggers SQL,
    /// pas par ces handlers. Il est testé dans les tests d'intégration (vraie base).
    /// </summary>
    public class AchatCommandHandlersTests
    {
        private readonly IAchatRepository _achatRepository = Substitute.For<IAchatRepository>();
        private readonly IBandeRepository _bandeRepository = Substitute.For<IBandeRepository>();

        // Bande : 100 poussins de départ + 20 achetés = 120 ; 15 morts ou vendus => 105 présents
        private readonly Bande _bande = new() { Id = 2, NbPoussinsInit = 120, NbPoussinsActuel = 105 };

        private readonly Achat _achatPoussins = new()
        {
            Id = 10,
            BandeId = 2,
            FournisseurId = 1,
            DateAchat = new DateTime(2026, 10, 10),
            TypeAchat = TypeAchat.Poussins,
            Quantite = 20,
            UniteMesure = UniteMesureAchat.Unite,
            PrixUnitaire = 500,
            MontantTotal = 10000
        };

        private readonly Achat _achatAliment = new()
        {
            Id = 11,
            BandeId = 2,
            FournisseurId = 1,
            DateAchat = new DateTime(2026, 10, 10),
            TypeAchat = TypeAchat.Aliment,
            Quantite = 50,
            UniteMesure = UniteMesureAchat.Kg,
            PrixUnitaire = 300,
            MontantTotal = 15000
        };

        public AchatCommandHandlersTests()
        {
            _achatRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(_achatPoussins);
            _achatRepository.GetByIdAsync(11, Arg.Any<CancellationToken>()).Returns(_achatAliment);
            _bandeRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(_bande);
        }

        private Task<AchatResponseDto> Modifier(UpdateAchatDto modification) =>
            new UpdateAchatCommandHandler(_achatRepository, _bandeRepository, NullLogger<UpdateAchatCommandHandler>.Instance)
                .Handle(new UpdateAchatCommand { UpdateAchatDto = modification }, CancellationToken.None);

        private Task Supprimer(int id) =>
            new DeleteAchatCommandHandler(_achatRepository, _bandeRepository, NullLogger<DeleteAchatCommandHandler>.Instance)
                .Handle(new DeleteAchatCommand { Id = id }, CancellationToken.None);

        // ---------- Modification ----------

        [Fact]
        public async Task Modification_QuantiteSeule_RecalculeLeMontant()
        {
            var resultat = await Modifier(new UpdateAchatDto { Id = 11, Quantite = 60 });

            Assert.Equal(18000, resultat.MontantTotal);   // 60 x 300
        }

        [Fact]
        public async Task Modification_PoussinsAugmentes_AjouteLaDifferenceALaBande()
        {
            await Modifier(new UpdateAchatDto { Id = 10, Quantite = 30 });   // 20 -> 30

            // Init ET Actuel bougent ensemble : les morts et vendus (Init - Actuel = 15) ne changent pas
            Assert.Equal(130, _bande.NbPoussinsInit);
            Assert.Equal(115, _bande.NbPoussinsActuel);
            await _bandeRepository.Received(1).UpdateAsync(_bande, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Modification_PoussinsDiminues_RetireLaDifferenceDeLaBande()
        {
            await Modifier(new UpdateAchatDto { Id = 10, Quantite = 5 });   // 20 -> 5

            Assert.Equal(105, _bande.NbPoussinsInit);
            Assert.Equal(90, _bande.NbPoussinsActuel);
        }

        [Fact]
        public async Task Modification_PoussinsDejaVendus_RefuseSansRienEcrire()
        {
            // Il ne reste que 3 poussins dans la bande : impossible d'en retirer 15 (20 -> 5)
            _bande.NbPoussinsActuel = 3;

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => Modifier(new UpdateAchatDto { Id = 10, Quantite = 5 }));

            await _achatRepository.DidNotReceive().UpdateAsync(Arg.Any<Achat>(), Arg.Any<CancellationToken>());
            await _bandeRepository.DidNotReceive().UpdateAsync(Arg.Any<Bande>(), Arg.Any<CancellationToken>());
            Assert.Equal(120, _bande.NbPoussinsInit);
        }

        [Fact]
        public async Task Modification_AchatAliment_NeTouchePasALaBande()
        {
            await Modifier(new UpdateAchatDto { Id = 11, Quantite = 80 });

            await _bandeRepository.DidNotReceive().UpdateAsync(Arg.Any<Bande>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Modification_DateReceptionSeuleAvantLAchat_RefuseeSansEcrire()
        {
            // Seule la date de réception est envoyée : le validateur ne connaît pas la date d'achat
            // enregistrée (10/10). C'est le handler qui fait la vérification.
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => Modifier(new UpdateAchatDto { Id = 11, DateReception = new DateTime(2026, 10, 5) }));

            await _achatRepository.DidNotReceive().UpdateAsync(Arg.Any<Achat>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Modification_AchatInexistant_LeveNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => Modifier(new UpdateAchatDto { Id = 999, Quantite = 1 }));
        }

        // ---------- Suppression ----------

        [Fact]
        public async Task Suppression_AchatDePoussins_RetireLesPoussinsDeLaBande()
        {
            await Supprimer(10);

            Assert.Equal(100, _bande.NbPoussinsInit);    // 120 - 20
            Assert.Equal(85, _bande.NbPoussinsActuel);   // 105 - 20
            await _achatRepository.Received(1).DeleteAsync(10, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Suppression_AchatAliment_NeTouchePasALaBande()
        {
            await Supprimer(11);

            await _bandeRepository.DidNotReceive().UpdateAsync(Arg.Any<Bande>(), Arg.Any<CancellationToken>());
            await _achatRepository.Received(1).DeleteAsync(11, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Suppression_AchatInexistant_LeveNotFoundSansRienSupprimer()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => Supprimer(999));

            await _achatRepository.DidNotReceive().DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }
}
