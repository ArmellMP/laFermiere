using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Utilisateurs.Interfaces
{
    public interface IUtilisateurRepository
    {
        // ============================================
        // LECTURES
        // ============================================
        
        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        Task<IEnumerable<Utilisateur>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère un utilisateur par son ID
        /// </summary>
        Task<Utilisateur?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les utilisateurs par rôle
        /// </summary>
        Task<IEnumerable<Utilisateur>> GetByRoleAsync(RoleUtilisateur role, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les utilisateurs actifs
        /// </summary>
        Task<IEnumerable<Utilisateur>> GetActifsAsync(CancellationToken cancellationToken = default);
        
        // ============================================
        // ÉCRITURES
        // ============================================
        
        /// <summary>
        /// Ajoute un nouvel utilisateur
        /// </summary>
        Task<Utilisateur> AddAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Met à jour un utilisateur existant
        /// </summary>
        Task UpdateAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Supprime un utilisateur
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Met à jour la date de dernière connexion
        /// </summary>
        Task UpdateDerniereConnexionAsync(int id, DateTime dateConnexion, CancellationToken cancellationToken = default);
        
        // ============================================
        // VÉRIFICATIONS
        // ============================================
        
        /// <summary>
        /// Vérifie si un utilisateur existe
        /// </summary>
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie si un email existe déjà
        /// </summary>
        Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie les identifiants de connexion et retourne l'utilisateur
        /// </summary>
        Task<Utilisateur?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
        }
}