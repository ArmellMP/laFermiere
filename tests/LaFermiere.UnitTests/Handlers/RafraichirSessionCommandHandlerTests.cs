using Application.Common.Security;
using Application.Features.Utilisateurs.Commands.RafraichirSession;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Renouvellement de session (refresh token) :
    ///   - rotation : le token est marqué utilisé, un nouveau est émis dans la même session
    ///   - réutilisation d'un token déjà échangé : refus + toute la session révoquée
    ///   - token expiré, révoqué ou inconnu : refus, sans rien émettre
    ///   - compte désactivé : refus + session révoquée
    /// </summary>
    public class RafraichirSessionCommandHandlerTests
    {
        private const string Token = "token-client";
        private const string Empreinte = "empreinte-du-token";
        private const string Famille = "famille-1";

        private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        private readonly IUtilisateurRepository _utilisateurs = Substitute.For<IUtilisateurRepository>();
        private readonly ISessionService _sessions = Substitute.For<ISessionService>();

        private readonly Utilisateur _utilisateur = new()
        {
            Id = 7, Nom = "Fermier", Email = "fermier@test.local", Role = RoleUtilisateur.Fermier, EstActif = true
        };

        public RafraichirSessionCommandHandlerTests()
        {
            _sessions.Empreinte(Token).Returns(Empreinte);
            _utilisateurs.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(_utilisateur);
            _refreshTokens.MarquerUtiliseAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(true);
            _sessions.EmettreAsync(_utilisateur, Famille, Arg.Any<CancellationToken>())
                .Returns(new JetonsSession("jwt", DateTime.UtcNow.AddMinutes(15), "nouveau-token", DateTime.UtcNow.AddDays(7)));
        }

        private RafraichirSessionCommandHandler Handler() =>
            new(_refreshTokens, _utilisateurs, _sessions, NullLogger<RafraichirSessionCommandHandler>.Instance);

        private void TokenStocke(DateTime? utiliseLe = null, DateTime? revoqueLe = null, DateTime? expireLe = null) =>
            _refreshTokens.GetByHashAsync(Empreinte, Arg.Any<CancellationToken>()).Returns(new RefreshToken
            {
                Id = 1,
                UtilisateurId = 7,
                TokenHash = Empreinte,
                FamilleId = Famille,
                ExpireLe = expireLe ?? DateTime.UtcNow.AddDays(3),
                UtiliseLe = utiliseLe,
                RevoqueLe = revoqueLe
            });

        private Task<UnauthorizedAccessException> Refus() =>
            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                Handler().Handle(new RafraichirSessionCommand { RefreshToken = Token }, CancellationToken.None));

        private Task AucuneEmission() =>
            _sessions.DidNotReceive().EmettreAsync(Arg.Any<Utilisateur>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

        [Fact]
        public async Task TokenValide_RotationDansLaMemeSession()
        {
            TokenStocke();

            var reponse = await Handler().Handle(new RafraichirSessionCommand { RefreshToken = Token }, CancellationToken.None);

            Assert.Equal("jwt", reponse.Token);
            Assert.Equal("nouveau-token", reponse.RefreshToken);
            await _refreshTokens.Received(1).MarquerUtiliseAsync(1, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
            await _sessions.Received(1).EmettreAsync(_utilisateur, Famille, Arg.Any<CancellationToken>());
            await _refreshTokens.DidNotReceive().RevoquerFamilleAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task TokenDejaUtilise_RefusEtSessionRevoquee()
        {
            TokenStocke(utiliseLe: DateTime.UtcNow.AddMinutes(-5));

            await Refus();

            await _refreshTokens.Received(1).RevoquerFamilleAsync(Famille, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
            await AucuneEmission();
        }

        [Fact]
        public async Task DeuxRequetesSimultanees_LaSecondeEstTraiteeCommeUneReutilisation()
        {
            TokenStocke();
            // Une autre requête a marqué le token entre la lecture et la mise à jour
            _refreshTokens.MarquerUtiliseAsync(1, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(false);

            await Refus();

            await _refreshTokens.Received(1).RevoquerFamilleAsync(Famille, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
            await AucuneEmission();
        }

        [Fact]
        public async Task TokenExpire_Refus()
        {
            TokenStocke(expireLe: DateTime.UtcNow.AddMinutes(-1));

            await Refus();
            await AucuneEmission();
        }

        [Fact]
        public async Task TokenRevoque_Refus()
        {
            TokenStocke(revoqueLe: DateTime.UtcNow.AddHours(-1));

            await Refus();
            await AucuneEmission();
        }

        [Fact]
        public async Task TokenInconnu_Refus()
        {
            await Refus();
            await AucuneEmission();
        }

        [Fact]
        public async Task CompteDesactive_RefusEtSessionRevoquee()
        {
            TokenStocke();
            _utilisateur.EstActif = false;

            await Refus();

            await _refreshTokens.Received(1).RevoquerFamilleAsync(Famille, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
            await AucuneEmission();
        }

        [Fact]
        public async Task TousLesRefus_MemeMessage()
        {
            TokenStocke(utiliseLe: DateTime.UtcNow);

            var exception = await Refus();

            Assert.Equal(RafraichirSessionCommandHandler.MessageRefus, exception.Message);
        }
    }
}
