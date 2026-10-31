using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Achats.Interfaces
{
    public interface IAchatRepository
    {
        // ============================================
        // LECTURES
        // ============================================
        
        /// <summary>
        /// Récupère tous les achats
        /// </summary>
        Task<IEnumerable<Achat>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère un achat par son ID
        /// </summary>
        Task<Achat?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les achats d'une bande spécifique
        /// </summary>
        Task<IEnumerable<Achat>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les achats d'un fournisseur spécifique
        /// </summary>
        Task<IEnumerable<Achat>> GetByFournisseurIdAsync(int fournisseurId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les achats par type
        /// </summary>
        Task<IEnumerable<Achat>> GetByTypeAsync(TypeAchat type, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les achats sur une période donnée
        /// </summary>
        Task<IEnumerable<Achat>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les achats non encore reçus
        /// </summary>
        Task<IEnumerable<Achat>> GetNonRecusAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcule le total des achats pour une bande
        /// </summary>
        Task<decimal> GetTotalAchatsByBandeAsync(int bandeId, CancellationToken cancellationToken = default);
        
        // ============================================
        // ÉCRITURES
        // ============================================
        
        /// <summary>
        /// Ajoute un nouvel achat
        /// </summary>
        Task<Achat> AddAsync(Achat achat, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Met à jour un achat existant
        /// </summary>
        Task UpdateAsync(Achat achat, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Supprime un achat
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        // ============================================
        // VÉRIFICATIONS
        // ============================================
        
        /// <summary>
        /// Vérifie si un achat existe
        /// </summary>
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie si un fournisseur a des achats
        /// </summary>
        Task<bool> HasAchatsByFournisseurAsync(int fournisseurId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie si une bande a des achats
        /// </summary>
        Task<bool> HasAchatsByBandeAsync(int bandeId, CancellationToken cancellationToken = default);
    }
}