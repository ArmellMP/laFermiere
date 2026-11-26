using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Features.Utilisateurs.Commands.SecuriserCompteInitial;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Mise en ligne : le compte admin@ferme.local / Admin123 de schema.sql (mot de passe public)
    /// est remplacé au premier démarrage par l'e-mail et le mot de passe choisis pour le serveur.
    /// </summary>
    public class SecuriserCompteInitialTests
    {
        private readonly IUtilisateurRepository _utilisateurs = Substitute.For<IUtilisateurRepository>();
        private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

        // Hash BCrypt de « Admin123 », comme dans schema.sql
        private readonly Utilisateur _adminParDefaut = new()
        {
            Id = 1, Nom = "Administrateur", Email = "admin@ferme.local", Role = RoleUtilisateur.Admin, EstActif = true,
            PasswordHash = "$2a$12$cno2DpEh3P/m54T4F0LDLOhnpxNd/JNoNvefsv3EwrXMk5GnzSv4e"
        };

        private Task<ResultatCompteInitial> Securiser(string email, string motDePasse, string? nom = null)
        {
            _utilisateurs.GetByRoleAsync(RoleUtilisateur.Admin, Arg.Any<CancellationToken>()).Returns([_adminParDefaut]);
            return new SecuriserCompteInitialCommandHandler(_utilisateurs, _refreshTokens, NullLogger<SecuriserCompteInitialCommandHandler>.Instance)
                .Handle(new SecuriserCompteInitialCommand(email, motDePasse, nom), CancellationToken.None);
        }

        [Fact]
        public async Task CompteParDefaut_RemplaceParLeCompteChoisi()
        {
            var resultat = await Securiser("gerant@lafermiere.cm", "Serveur2026!", "Armell");

            Assert.Equal(ResultatCompteInitial.Securise, resultat);
            Assert.Equal("gerant@lafermiere.cm", _adminParDefaut.Email);
            Assert.Equal("Armell", _adminParDefaut.Nom);
            Assert.True(PasswordHasher.VerifyPassword("Serveur2026!", _adminParDefaut.PasswordHash));
            Assert.False(PasswordHasher.VerifyPassword("Admin123", _adminParDefaut.PasswordHash));
            await _utilisateurs.Received(1).UpdateAsync(_adminParDefaut, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DejaSecurise_AuDeuxiemeDemarrage_RienNeChange()
        {
            _adminParDefaut.PasswordHash = PasswordHasher.HashPassword("Change2026");

            var resultat = await Securiser("gerant@lafermiere.cm", "Serveur2026!");

            Assert.Equal(ResultatCompteInitial.DejaSecurise, resultat);
            await _utilisateurs.DidNotReceive().UpdateAsync(Arg.Any<Utilisateur>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("court1A")]       // moins de 8 caractères
        [InlineData("sansmajuscule1")]
        [InlineData("SANSMINUSCULE1")]
        [InlineData("SansChiffre")]
        [InlineData("Admin123")]      // l'ancien mot de passe public
        public async Task MotDePasseTropFaible_DemarrageRefuse(string motDePasse)
        {
            await Assert.ThrowsAsync<BusinessRuleException>(() => Securiser("gerant@lafermiere.cm", motDePasse));
            await _utilisateurs.DidNotReceive().UpdateAsync(Arg.Any<Utilisateur>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EmailDejaPrisParUnAutreCompte_DemarrageRefuse()
        {
            _utilisateurs.EmailExistsAsync("pris@lafermiere.cm", 1, Arg.Any<CancellationToken>()).Returns(true);

            await Assert.ThrowsAsync<ConflictException>(() => Securiser("pris@lafermiere.cm", "Serveur2026!"));
            await _utilisateurs.DidNotReceive().UpdateAsync(Arg.Any<Utilisateur>(), Arg.Any<CancellationToken>());
        }
    }
}
