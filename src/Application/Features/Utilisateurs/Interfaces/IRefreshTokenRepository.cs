using Domain.Entities;

namespace Application.Features.Utilisateurs.Interfaces
{
    /// <summary>
    /// Stockage des refresh tokens (table RefreshTokens). Les dates sont en UTC.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        /// <summary>Token correspondant à cette empreinte SHA-256, ou null.</summary>
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marque le token comme utilisé, SEULEMENT s'il ne l'était pas déjà et n'est pas révoqué.
        /// Renvoie false si une autre requête l'a utilisé entre-temps : l'opération est atomique
        /// (un seul UPDATE), deux renouvellements simultanés ne peuvent pas réussir tous les deux.
        /// </summary>
        Task<bool> MarquerUtiliseAsync(int id, DateTime dateUtc, CancellationToken cancellationToken = default);

        /// <summary>Révoque tous les tokens encore valides d'une session.</summary>
        Task RevoquerFamilleAsync(string familleId, DateTime dateUtc, CancellationToken cancellationToken = default);

        /// <summary>Révoque toutes les sessions d'un utilisateur.</summary>
        Task RevoquerToutPourUtilisateurAsync(int utilisateurId, DateTime dateUtc, CancellationToken cancellationToken = default);

        /// <summary>Supprime les tokens expirés de l'utilisateur (ménage à chaque connexion).</summary>
        Task SupprimerExpiresAsync(int utilisateurId, DateTime maintenantUtc, CancellationToken cancellationToken = default);
    }
}
