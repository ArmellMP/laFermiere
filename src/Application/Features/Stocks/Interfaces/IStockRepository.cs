using Domain.Entities;

namespace Application.Features.Stocks.Interfaces
{
    public interface IStockRepository
    {
        // ====================================================
        // LECTURES (requêtes qui ne modifient pas les données)
        // ====================================================
        
        /// <summary>
        /// Récupère TOUS les stocks (sans filtre)
        /// </summary>
        Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default);
        // IEnumerable<Stock> : une liste qu'on peut parcourir
        // Task : indique que l'opération est asynchrone (ne bloque pas le thread)
        // cancellationToken : paramètre optionnel pour annuler une longue opération

        /// <summary>
        /// Récupère tous les stocks d'une bande spécifique
        /// </summary>
        Task<IEnumerable<Stock>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Récupère un stock en particulier par son ID ( ici c'est ID du stock qui est unique ) 
        /// </summary>
        Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Récupère tous les stocks qui sont en dessous de leur seuil d'alerte
        /// </summary>
        Task<IEnumerable<Stock>> GetEnAlerteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Récupère tous les stocks dont la date de péremption est dépassée
        /// </summary>
        Task<IEnumerable<Stock>> GetPerimesAsync(CancellationToken cancellationToken = default);

        // ============================================
        // ÉCRITURES (opérations qui modifient les données)
        // ============================================
        
        /// <summary>
        /// Ajoute un nouveau stock dans la base de données; Le stock créé AVEC son ID généré par la BDD
        /// </summary>
        Task<Stock> AddAsync(Stock stock, CancellationToken cancellationToken = default);

        /// <summary>
        /// Met à jour un stock existant
        /// </summary>
        Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default);

        /// <summary>
        /// Supprime un stock de la base de données
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // ============================================
        // VÉRIFICATIONS ET UTILITAIRES
        // ============================================
        
        /// <summary>
        /// Vérifie si un stock existe dans la base
        /// </summary>
        /// <returns>true si existe, false sinon</returns>
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Vérifie si un stock a déjà été consommé (utilisé dans des suivis journaliers)
        /// Utile pour empêcher la suppression d'un stock utilisé
        /// </summary>
        Task<bool> HasConsommationAsync(int stockId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Récupère la quantité disponible d'un stock; c'est la quantité initiale moins la quantité consommée
        /// </summary>
        Task<decimal> GetQuantiteDisponibleAsync(int stockId, CancellationToken cancellationToken = default);
    }
}