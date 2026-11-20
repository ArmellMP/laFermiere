using Application.Common.Exceptions;
using Application.Features.Utilisateurs.Commands.DeleteUtilisateur;
using Application.Features.Utilisateurs.Commands.UpdateUtilisateur;
using Application.Features.Utilisateurs.Dtos;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Regles;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LaFermiere.UnitTests.Handlers
{
    /// <summary>
    /// Il doit toujours rester au moins un administrateur actif : sinon plus personne ne peut
    /// gérer l'application (créer un compte, réactiver un utilisateur…).
    /// Supprimer, désactiver ou rétrograder le dernier administrateur actif est refusé, sans rien écrire.
    /// </summary>
    public class RegleAdministrateursTests
    {
        private readonly IUtilisateurRepository _utilisateurs = Substitute.For<IUtilisateurRepository>();
        private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

        private readonly Utilisateur _admin = new() { Id = 1, Nom = "Admin", Email = "admin@test.local", Role = RoleUtilisateur.Admin, EstActif = true };
        private readonly Utilisateur _autreAdmin = new() { Id = 2, Nom = "Autre admin", Email = "autre@test.local", Role = RoleUtilisateur.Admin, EstActif = true };
        private readonly Utilisateur _fermier = new() { Id = 3, Nom = "Fermier", Email = "fermier@test.local", Role = RoleUtilisateur.Fermier, EstActif = true };

        public RegleAdministrateursTests()
        {
            foreach (var u in new[] { _admin, _autreAdmin, _fermier })
                _utilisateurs.GetByIdAsync(u.Id, Arg.Any<CancellationToken>()).Returns(u);
        }

        /// <summary>Les administrateurs présents en base pour ce test.</summary>
        private void Admins(params Utilisateur[] admins) =>
            _utilisateurs.GetByRoleAsync(RoleUtilisateur.Admin, Arg.Any<CancellationToken>()).Returns(admins);

        private Task<UtilisateurResponseDto> Modifier(UpdateUtilisateurDto dto) =>
            new UpdateUtilisateurCommandHandler(_utilisateurs, _refreshTokens, NullLogger<UpdateUtilisateurCommandHandler>.Instance)
                .Handle(new UpdateUtilisateurCommand { UpdateUtilisateurDto = dto }, CancellationToken.None);

        private Task Supprimer(int id) =>
            new DeleteUtilisateurCommandHandler(_utilisateurs, NullLogger<DeleteUtilisateurCommandHandler>.Instance)
                .Handle(new DeleteUtilisateurCommand { Id = id }, CancellationToken.None);

        [Fact]
        public async Task Desactiver_LeDernierAdminActif_EstRefuse()
        {
            Admins(_admin);

            var erreur = await Assert.ThrowsAsync<BusinessRuleException>(() => Modifier(new UpdateUtilisateurDto { Id = 1, EstActif = false }));

            Assert.Equal(RegleAdministrateurs.Message, erreur.Message);
            await _utilisateurs.DidNotReceive().UpdateAsync(Arg.Any<Utilisateur>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Retrograder_LeDernierAdminActif_EstRefuse()
        {
            Admins(_admin);

            await Assert.ThrowsAsync<BusinessRuleException>(() => Modifier(new UpdateUtilisateurDto { Id = 1, Role = "Fermier" }));
            await _utilisateurs.DidNotReceive().UpdateAsync(Arg.Any<Utilisateur>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UnAdminInactif_NeComptePasCommeAdminRestant()
        {
            Admins(_admin, new Utilisateur { Id = 9, Role = RoleUtilisateur.Admin, EstActif = false });

            await Assert.ThrowsAsync<BusinessRuleException>(() => Modifier(new UpdateUtilisateurDto { Id = 1, EstActif = false }));
        }

        [Fact]
        public async Task Desactiver_UnAdmin_QuandUnAutreResteActif_EstAccepte()
        {
            Admins(_admin, _autreAdmin);

            var resultat = await Modifier(new UpdateUtilisateurDto { Id = 1, EstActif = false });

            Assert.False(resultat.EstActif);
            await _utilisateurs.Received(1).UpdateAsync(_admin, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ChangerLeMotDePasse_DuDernierAdmin_EstAccepte()
        {
            // Seuls la désactivation et le changement de rôle sont concernés
            Admins(_admin);

            await Modifier(new UpdateUtilisateurDto { Id = 1, Password = "Nouveau123" });

            await _utilisateurs.Received(1).UpdateAsync(_admin, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Supprimer_LeDernierAdminActif_EstRefuse()
        {
            Admins(_admin);

            await Assert.ThrowsAsync<BusinessRuleException>(() => Supprimer(1));
            await _utilisateurs.DidNotReceive().DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Supprimer_UnFermier_NeConsultePasLesAdmins()
        {
            await Supprimer(3);

            await _utilisateurs.Received(1).DeleteAsync(3, Arg.Any<CancellationToken>());
            await _utilisateurs.DidNotReceive().GetByRoleAsync(Arg.Any<RoleUtilisateur>(), Arg.Any<CancellationToken>());
        }
    }
}
