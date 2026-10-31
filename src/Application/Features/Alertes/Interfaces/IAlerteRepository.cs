using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Alertes.Interfaces
{
    public interface IAlerteRepository
    {
        // ============================================
        // LECTURES - RECHERCHES DE BASE
        // ============================================
        
        /// <summary>
        /// Récupère toutes les alertes
        /// </summary>
        Task<IEnumerable<Alerte>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère une alerte par son ID
        /// </summary>
        Task<Alerte?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        // ============================================
        // FILTRES PAR STATUT
        // ============================================
        
        /// <summary>
        /// Récupère toutes les alertes non lues
        /// </summary>
        Task<IEnumerable<Alerte>> GetNonLuesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère toutes les alertes lues
        /// </summary>
        Task<IEnumerable<Alerte>> GetLuesAsync(CancellationToken cancellationToken = default);
        
        // ============================================
        // FILTRES PAR TYPE
        // ============================================
        
        /// <summary>
        /// Récupère les alertes par type
        /// </summary>
        Task<IEnumerable<Alerte>> GetByTypeAsync(TypeAlerte type, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les alertes par niveau de criticité
        /// </summary>
        Task<IEnumerable<Alerte>> GetByNiveauAsync(NiveauAlerte niveau, CancellationToken cancellationToken = default);
        
        // ============================================
        // FILTRES PAR ENTITÉ
        // ============================================
        
        /// <summary>
        /// Récupère les alertes pour une entité spécifique (bande, stock, etc.)
        /// </summary>
        Task<IEnumerable<Alerte>> GetByEntiteAsync(int entiteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les alertes pour une entité et un type spécifiques
        /// </summary>
        Task<IEnumerable<Alerte>> GetByEntiteAndTypeAsync(int entiteId, TypeAlerte type, CancellationToken cancellationToken = default);
        
        // ============================================
        // FILTRES PAR PÉRIODE
        // ============================================
        
        /// <summary>
        /// Récupère les alertes sur une période donnée
        /// </summary>
        Task<IEnumerable<Alerte>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les alertes des dernières 24h
        /// </summary>
        Task<IEnumerable<Alerte>> GetLast24hAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les alertes des 7 derniers jours
        /// </summary>
        Task<IEnumerable<Alerte>> GetLast7DaysAsync(CancellationToken cancellationToken = default);
        
        // ============================================
        // STATISTIQUES
        // ============================================
        
        /// <summary>
        /// Compte le nombre total d'alertes non lues
        /// </summary>
        Task<int> CountNonLuesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Compte le nombre d'alertes par type
        /// </summary>
        Task<Dictionary<TypeAlerte, int>> CountByTypeAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Compte le nombre d'alertes par niveau
        /// </summary>
        Task<Dictionary<NiveauAlerte, int>> CountByNiveauAsync(CancellationToken cancellationToken = default);
        
        // ============================================
        // ÉCRITURES
        // ============================================
        
        /// <summary>
        /// Crée une nouvelle alerte
        /// </summary>
        Task<Alerte> AddAsync(Alerte alerte, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Met à jour une alerte (principalement pour marquer comme lue)
        /// </summary>
        Task UpdateAsync(Alerte alerte, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marque une alerte comme lue
        /// </summary>
        Task MarquerCommeLueAsync(int alerteId, int utilisateurId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marque plusieurs alertes comme lues
        /// </summary>
        Task MarquerPlusieursCommeLuesAsync(IEnumerable<int> alerteIds, int utilisateurId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marque toutes les alertes comme lues
        /// </summary>
        Task MarquerToutesCommeLuesAsync(int utilisateurId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Supprime une alerte
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Supprime les alertes plus anciennes qu'une certaine date
        /// </summary>
        Task DeleteOldAsync(DateTime dateLimite, CancellationToken cancellationToken = default);
        
        // ============================================
        // GÉNÉRATION D'ALERTES MÉTIER
        // ============================================
        
        /// <summary>
        /// Vérifie et crée des alertes pour les stocks faibles
        /// </summary>
        Task CheckAndCreateStockFaibleAlertesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie et crée des alertes pour les produits proches de la péremption
        /// </summary>
        Task CheckAndCreatePeremptionAlertesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie et crée des alertes pour les bandes à clôturer
        /// </summary>
        Task CheckAndCreateBandeACloturerAlertesAsync(CancellationToken cancellationToken = default);
    }
}