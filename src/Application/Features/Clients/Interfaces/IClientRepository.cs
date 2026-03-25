// Application/Features/Clients/Interfaces/IClientRepository.cs
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Clients.Interfaces
{
    public interface IClientRepository
    {
        // ==== LECTURES ====
        Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetByNomAsync(string nom, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetByTypeAsync(TypeClient type, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetActifsAsync(CancellationToken cancellationToken = default);
        
        // ==== ÉCRITURES ====
        Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);
        Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        // ==== VÉRIFICATIONS ====
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie si un email existe déjà dans la base, ID à exclure (pour les mises à jour), true si l'email existe, false sinon
        /// </summary>
  
        Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
        
        Task<bool> HasVentesAsync(int clientId, CancellationToken cancellationToken = default);
    }
}