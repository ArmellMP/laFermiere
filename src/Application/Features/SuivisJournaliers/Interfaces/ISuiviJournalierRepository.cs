using Domain.Entities;

namespace Application.Features.SuivisJournaliers.Interfaces
{
    public interface ISuiviJournalierRepository
    { 
        /// <summary>
        /// Récupère tous les suivis d'une bande, triés par jour
        /// </summary>
        Task<IEnumerable<SuiviJournalier>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère un suivi par son ID
        /// </summary>
        Task<SuiviJournalier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère le dernier suivi d'une bande
        /// </summary>
        Task<SuiviJournalier?> GetLastByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les suivis d'une bande pour une période donnée
        /// </summary>
        Task<IEnumerable<SuiviJournalier>> GetByBandeIdAndDateRangeAsync(
            int bandeId, 
            DateTime dateDebut, 
            DateTime dateFin, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie si un suivi existe déjà pour une bande à une date donnée
        /// </summary>
        Task<bool> ExistsForDateAsync(int bandeId, DateTime dateSuivi, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère tous les suivis où la température était anormale Pour générer des alertes
        /// </summary>
        Task<IEnumerable<SuiviJournalier>> GetWithAbnormalTemperatureAsync(
            decimal? minTemp = null, 
            decimal? maxTemp = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Récupère tous les suivis avec mortalité élevée Pour analyse des problèmes de santé
        /// </summary>
        Task<IEnumerable<SuiviJournalier>> GetWithHighMortalityAsync(
            int seuilMortalite = 5, 
            CancellationToken cancellationToken = default);

        // ============================================
        // ÉCRITURES
        // ============================================
        
        /// <summary>
        /// Ajoute un nouveau suivi
        /// </summary>
        Task<SuiviJournalier> AddAsync(SuiviJournalier suivi, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Met à jour un suivi existant
        /// </summary>
        Task UpdateAsync(SuiviJournalier suivi, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Supprime un suivi
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // ============================================
        // AGRÉGATIONS / STATISTIQUES
        // ============================================
        
        /// <summary>
        /// Calcule le poids moyen sur une période, Pour suivre la croissance
        /// </summary>
        Task<decimal?> GetAveragePoidsByBandeIdAndDateRangeAsync(
            int bandeId, 
            DateTime dateDebut, 
            DateTime dateFin, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcule le total des mortalités sur une période Pour analyser la survie
        /// </summary>
        Task<int> GetTotalMortalitesByBandeIdAndDateRangeAsync(
            int bandeId, 
            DateTime dateDebut, 
            DateTime dateFin, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les jours où des soins ont été administrés Pour traçabilité
        /// </summary>
        Task<IEnumerable<SuiviJournalier>> GetWithSoinsAsync(
            int bandeId, 
            CancellationToken cancellationToken = default);
    }
}