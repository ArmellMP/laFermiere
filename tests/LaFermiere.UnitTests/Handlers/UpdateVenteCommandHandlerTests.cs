using Application.Common.Exceptions;
using Application.Features.Bandes.Interfaces;
using Application.Features.Ventes.Commands.UpdateVente;
using Application.Features.Ventes.Dtos;
using Application.Features.Ventes.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Tests de la modification d'une vente (bug corrigé le 2026-11-01, commit 088a6ae).
    ///
    /// Règles protégées :
    ///   - le montant total est TOUJOURS recalculé (sinon erreur SQL chk_montant_coherent)
    ///   - l'effectif de la bande reçoit la DIFFÉRENCE de poulets vendus
    ///   - refus si la bande n'a pas assez de poulets, sans rien écrire
    ///
    /// LES MOCKS (NSubstitute)
    /// Le handler a besoin de deux repositories. Au lieu d'une vraie base de données,
    /// on lui donne des "substituts" :
    ///   Substitute.For&lt;IVenteRepository&gt;()   crée un faux repository
    ///   repo.GetByIdAsync(...).Returns(vente)    décide de ce qu'il renvoie
    ///   await repo.Received(1).UpdateAsync(...)  vérifie qu'il a été appelé une fois
    ///   await repo.DidNotReceive().UpdateAsync(...) vérifie qu'il n'a PAS été appelé
    /// Avantages : test rapide, sans base, et on contrôle exactement la situation de départ.
    /// </summary>
    public class UpdateVenteCommandHandlerTests
    {
        private readonly IVenteRepository _venteRepository = Substitute.For<IVenteRepository>();
        private readonly IBandeRepository _bandeRepository = Substitute.For<IBandeRepository>();
        private readonly UpdateVenteCommandHandler _handler;

        // Situation de départ commune : une bande de 50 poulets, dont 10 déjà vendus (reste 40)
        private readonly Bande _bande = new() { Id = 7, NbPoussinsInit = 50, NbPoussinsActuel = 40 };
        private readonly Vente _vente = new()
        {
            Id = 3,
            BandeId = 7,
            TypeVente = TypeVente.PouletsVivants,
            Quantite = 10,
            UniteMesure = UniteMesureVente.Unite,
            PrixUnitaire = 2500,
            MontantTotal = 25000,
            ModePaiement = ModePaiement.Especes
        };

        public UpdateVenteCommandHandlerTests()
        {
            // Le constructeur de la classe de test s'exécute avant CHAQUE test : il prépare les mocks.
            // Arg.Any<CancellationToken>() : "quelle que soit la valeur de ce paramètre".
            _venteRepository.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns(_vente);
            _bandeRepository.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(_bande);

            // NullLogger : un logger qui n'écrit nulle part (les logs ne nous intéressent pas ici)
            _handler = new UpdateVenteCommandHandler(
                _venteRepository, _bandeRepository, NullLogger<UpdateVenteCommandHandler>.Instance);
        }

        private Task<VenteResponseDto> Modifier(UpdateVenteDto modification) =>
            _handler.Handle(new UpdateVenteCommand { UpdateVenteDto = modification }, CancellationToken.None);

        [Fact]
        public async Task QuantiteAugmentee_RetireLaDifferenceALaBande()
        {
            // Act : 10 -> 12 poulets vendus
            await Modifier(new UpdateVenteDto { Id = 3, Quantite = 12 });

            // Assert : 2 poulets de plus quittent la bande (40 -> 38)
            Assert.Equal(38, _bande.NbPoussinsActuel);
            // Arg.Is(...) : vérifie le contenu de l'objet passé au repository
            await _bandeRepository.Received(1).UpdateAsync(
                Arg.Is<Bande>(b => b.Id == 7 && b.NbPoussinsActuel == 38), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task QuantiteDiminuee_RendLaDifferenceALaBande()
        {
            await Modifier(new UpdateVenteDto { Id = 3, Quantite = 4 });

            Assert.Equal(46, _bande.NbPoussinsActuel);   // 6 poulets reviennent
        }

        [Fact]
        public async Task QuantiteSeule_RecalculeLeMontant()
        {
            // Le bug d'origine : seul un changement de PRIX recalculait le montant
            var resultat = await Modifier(new UpdateVenteDto { Id = 3, Quantite = 12 });

            Assert.Equal(30000, resultat.MontantTotal);   // 12 x 2500
            await _venteRepository.Received(1).UpdateAsync(
                Arg.Is<Vente>(v => v.MontantTotal == 30000), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SansChangementDeQuantite_NeTouchePasALaBande()
        {
            await Modifier(new UpdateVenteDto { Id = 3, Observations = "client fidèle" });

            await _bandeRepository.DidNotReceive().UpdateAsync(Arg.Any<Bande>(), Arg.Any<CancellationToken>());
            Assert.Equal(40, _bande.NbPoussinsActuel);
        }

        [Fact]
        public async Task EffectifInsuffisant_RefuseSansRienEcrire()
        {
            // 10 -> 100 : il faudrait 90 poulets de plus, la bande n'en a que 40
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => Modifier(new UpdateVenteDto { Id = 3, Quantite = 100 }));

            // Le point essentiel : AUCUNE écriture, ni sur la vente ni sur la bande
            await _venteRepository.DidNotReceive().UpdateAsync(Arg.Any<Vente>(), Arg.Any<CancellationToken>());
            await _bandeRepository.DidNotReceive().UpdateAsync(Arg.Any<Bande>(), Arg.Any<CancellationToken>());
            Assert.Equal(40, _bande.NbPoussinsActuel);
        }

        [Fact]
        public async Task PassageEnFientes_RendTousLesPouletsALaBande()
        {
            // Les fientes ne retirent pas de poulets : les 10 poulets de la vente reviennent
            await Modifier(new UpdateVenteDto { Id = 3, TypeVente = "Fientes", UniteMesure = "Kg" });

            Assert.Equal(50, _bande.NbPoussinsActuel);
        }

        [Fact]
        public async Task VenteInexistante_LeveNotFound()
        {
            // Aucun Returns configuré pour l'Id 99 : NSubstitute renvoie null par défaut
            await Assert.ThrowsAsync<NotFoundException>(
                () => Modifier(new UpdateVenteDto { Id = 99, Quantite = 1 }));
        }
    }
}
