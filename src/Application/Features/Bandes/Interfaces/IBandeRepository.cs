using Domain.Entities;
using Application.Features.Bandes.Dtos;

namespace Application.Features.Bandes.Interfaces
{
    public interface IBandeRepository
    {
        /// <summary>
        /// Récupère toutes les bandes.
        /// </summary>
        Task<IEnumerable<Bande>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère une bande par son ID.
        /// </summary> 
        Task<Bande?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère une bande par son code.
        /// </summary>
        Task<Bande?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les bandes par statut.
        /// </summary>
        Task<IEnumerable<Bande>> GetByStatutAsync(Domain.Enums.StatutBande statut, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ajoute une nouvelle bande.
        /// </summary>
        Task<Bande> AddAsync(Bande bande, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Met à jour une bande existante.
        /// </summary>
        Task UpdateAsync(Bande bande, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Supprime une bande.
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Vérifie si un code de bande existe déjà.
        /// </summary>
        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Récupère les statistiques des bandes.
        /// </summary>
        Task<BandeStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    }
}