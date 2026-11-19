using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Ventes.Interfaces
{
    public interface IVenteRepository
    {
        // ==== LECTURES ====
        Task<IEnumerable<Vente>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Vente?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vente>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vente>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vente>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalVentesByBandeAsync(int bandeId, CancellationToken cancellationToken = default);

        /// <summary>Nombre de ventes d'un client et leur montant total (0 et 0 s'il n'en a aucune).</summary>
        Task<(int Nombre, decimal MontantTotal)> GetStatsByClientAsync(int clientId, CancellationToken cancellationToken = default);

        // ==== ÉCRITURES ====
        Task<Vente> AddAsync(Vente vente, CancellationToken cancellationToken = default);
        Task UpdateAsync(Vente vente, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        // ==== VÉRIFICATIONS ====
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasVentesByBandeAsync(int bandeId, CancellationToken cancellationToken = default);
        Task<bool> HasVentesByClientAsync(int clientId, CancellationToken cancellationToken = default);
    }
}