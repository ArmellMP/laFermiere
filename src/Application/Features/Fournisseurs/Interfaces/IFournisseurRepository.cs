// Application/Features/Fournisseurs/Interfaces/IFournisseurRepository.cs
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Fournisseurs.Interfaces
{
    public interface IFournisseurRepository
    {
        // ==== LECTURES ====
        Task<IEnumerable<Fournisseur>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Fournisseur?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Fournisseur>> GetByNomAsync(string nom, CancellationToken cancellationToken = default);
        Task<IEnumerable<Fournisseur>> GetByTypeAsync(TypeFournisseur type, CancellationToken cancellationToken = default);
        Task<IEnumerable<Fournisseur>> GetActifsAsync(CancellationToken cancellationToken = default);
        
        // ==== ÉCRITURES ====
        Task<Fournisseur> AddAsync(Fournisseur fournisseur, CancellationToken cancellationToken = default);
        Task UpdateAsync(Fournisseur fournisseur, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        // ==== VÉRIFICATIONS ====
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasAchatsAsync(int fournisseurId, CancellationToken cancellationToken = default);
    }
} 