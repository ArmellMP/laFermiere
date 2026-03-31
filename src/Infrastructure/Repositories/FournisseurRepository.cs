using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Fournisseurs.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    public class FournisseurRepository : IFournisseurRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<FournisseurRepository> _logger;

        public FournisseurRepository(DapperContext context, ILogger<FournisseurRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Fournisseur>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeFournisseur, Telephone, Email, Adresse,
                    ContactPersonne, Notes, EstActif, CreatedAt, UpdatedAt
                FROM Fournisseurs
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseurs = await connection.QueryAsync<RepFournisseurEntity>(sql);
            return repFournisseurs.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Fournisseur?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeFournisseur, Telephone, Email, Adresse,
                    ContactPersonne, Notes, EstActif, CreatedAt, UpdatedAt
                FROM Fournisseurs
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseur = await connection.QueryFirstOrDefaultAsync<RepFournisseurEntity>(sql, new { Id = id });
            return repFournisseur?.ToDomain();
        }

        public async Task<IEnumerable<Fournisseur>> GetByNomAsync(string nom, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeFournisseur, Telephone, Email, Adresse,
                    ContactPersonne, Notes, EstActif, CreatedAt, UpdatedAt
                FROM Fournisseurs
                WHERE Nom LIKE @Nom
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseurs = await connection.QueryAsync<RepFournisseurEntity>(sql, new { Nom = $"%{nom}%" });
            return repFournisseurs.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Fournisseur>> GetByTypeAsync(TypeFournisseur type, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeFournisseur, Telephone, Email, Adresse,
                    ContactPersonne, Notes, EstActif, CreatedAt, UpdatedAt
                FROM Fournisseurs
                WHERE TypeFournisseur = @TypeFournisseur
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseurs = await connection.QueryAsync<RepFournisseurEntity>(sql, new { TypeFournisseur = type.ToString() });
            return repFournisseurs.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Fournisseur>> GetActifsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeFournisseur, Telephone, Email, Adresse,
                    ContactPersonne, Notes, EstActif, CreatedAt, UpdatedAt
                FROM Fournisseurs
                WHERE EstActif = 1
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseurs = await connection.QueryAsync<RepFournisseurEntity>(sql);
            return repFournisseurs.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Fournisseur> AddAsync(Fournisseur fournisseur, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Fournisseurs (
                    Nom, TypeFournisseur, Telephone, Email, Adresse,
                    ContactPersonne, Notes, EstActif, CreatedAt, UpdatedAt
                ) VALUES (
                    @Nom, @TypeFournisseur, @Telephone, @Email, @Adresse,
                    @ContactPersonne, @Notes, @EstActif, @CreatedAt, @UpdatedAt
                );
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseur = RepFournisseurEntity.FromDomain(fournisseur);
            var id = await connection.ExecuteScalarAsync<int>(sql, repFournisseur);
            return await GetByIdAsync(id, cancellationToken) ?? fournisseur;
        }

        public async Task UpdateAsync(Fournisseur fournisseur, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Fournisseurs 
                SET 
                    Nom = @Nom,
                    TypeFournisseur = @TypeFournisseur,
                    Telephone = @Telephone,
                    Email = @Email,
                    Adresse = @Adresse,
                    ContactPersonne = @ContactPersonne,
                    Notes = @Notes,
                    EstActif = @EstActif,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repFournisseur = RepFournisseurEntity.FromDomain(fournisseur);
            var affectedRows = await connection.ExecuteAsync(sql, repFournisseur);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour le fournisseur {FournisseurId}", fournisseur.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Fournisseurs WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Fournisseurs WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> HasAchatsAsync(int fournisseurId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Achats WHERE FournisseurId = @FournisseurId";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { FournisseurId = fournisseurId });
            return count > 0;
        }
    }
}