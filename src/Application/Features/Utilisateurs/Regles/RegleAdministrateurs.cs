using Application.Common.Exceptions;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Enums;

namespace Application.Features.Utilisateurs.Regles
{
    /// <summary>
    /// Il doit toujours rester au moins un administrateur actif : sans lui, plus personne
    /// ne peut créer de compte, réactiver un utilisateur ou gérer l'application.
    /// Vérifié avant de supprimer, désactiver ou rétrograder un administrateur actif.
    /// </summary>
    public static class RegleAdministrateurs
    {
        public const string Message =
            "Il doit rester au moins un administrateur actif. Donnez d'abord le rôle Administrateur à un autre compte.";

        public static async Task VerifierQuUnAutreAdminResteAsync(
            IUtilisateurRepository repository,
            int utilisateurId,
            CancellationToken cancellationToken)
        {
            var admins = await repository.GetByRoleAsync(RoleUtilisateur.Admin, cancellationToken);
            if (!admins.Any(a => a.EstActif && a.Id != utilisateurId))
            {
                throw new BusinessRuleException(Message);
            }
        }
    }
}
