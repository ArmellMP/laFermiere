using MediatR;

namespace Application.Features.Utilisateurs.Commands.SecuriserCompteInitial
{
    /// <summary>
    /// Remplace le compte administrateur par défaut de database/schema.sql
    /// (admin@ferme.local / Admin123, mot de passe public publié dans le dépôt)
    /// par l'e-mail et le mot de passe choisis pour le serveur.
    /// Exécutée au démarrage de l'API quand CompteInitial:MotDePasse est configuré.
    /// Sans effet si le compte par défaut n'existe plus ou a déjà changé de mot de passe.
    /// </summary>
    public record SecuriserCompteInitialCommand(string Email, string MotDePasse, string? Nom) : IRequest<ResultatCompteInitial>;

    public enum ResultatCompteInitial
    {
        /// <summary>Le compte par défaut a été remplacé.</summary>
        Securise,
        /// <summary>Aucun compte avec le mot de passe par défaut : rien à faire.</summary>
        DejaSecurise
    }
}
