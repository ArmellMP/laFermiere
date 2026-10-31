using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Parametres.Interfaces
{
    public interface IParametreRepository
    {
        // ==== LECTURES ====
        Task<IEnumerable<Parametre>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Parametre?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Parametre?> GetByCleAsync(string cle, CancellationToken cancellationToken = default);
        Task<IEnumerable<Parametre>> GetByCategorieAsync(CategorieParametre categorie, CancellationToken cancellationToken = default);
        Task<IEnumerable<Parametre>> GetSystemesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Parametre>> GetModifiablesAsync(CancellationToken cancellationToken = default);
        
        // ==== ÉCRITURES ====
        Task<Parametre> AddAsync(Parametre parametre, CancellationToken cancellationToken = default);
        Task UpdateAsync(Parametre parametre, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        // ==== VÉRIFICATIONS ====
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> CleExistsAsync(string cle, int? excludeId = null, CancellationToken cancellationToken = default);
        
        // ==== UTILITAIRES ====
        Task<T?> GetValueAsync<T>(string cle, T? defaultValue = default, CancellationToken cancellationToken = default);
        Task<string?> GetValueAsync(string cle, CancellationToken cancellationToken = default);
    }
}