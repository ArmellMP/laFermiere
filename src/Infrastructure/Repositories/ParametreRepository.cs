using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Parametres.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    public class ParametreRepository : IParametreRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<ParametreRepository> _logger;

        public ParametreRepository(DapperContext context, ILogger<ParametreRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Parametre>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt
                FROM Parametres
                ORDER BY Categorie, Cle";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametres = await connection.QueryAsync<RepParametreEntity>(sql);
            return repParametres.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Parametre?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt
                FROM Parametres
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametre = await connection.QueryFirstOrDefaultAsync<RepParametreEntity>(sql, new { Id = id });
            return repParametre?.ToDomain();
        }

        public async Task<Parametre?> GetByCleAsync(string cle, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt
                FROM Parametres
                WHERE Cle = @Cle";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametre = await connection.QueryFirstOrDefaultAsync<RepParametreEntity>(sql, new { Cle = cle });
            return repParametre?.ToDomain();
        }

        public async Task<IEnumerable<Parametre>> GetByCategorieAsync(CategorieParametre categorie, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt
                FROM Parametres
                WHERE Categorie = @Categorie
                ORDER BY Cle";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametres = await connection.QueryAsync<RepParametreEntity>(sql, new { Categorie = categorie.ToString() });
            return repParametres.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Parametre>> GetSystemesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt
                FROM Parametres
                WHERE EstModifiable = 0
                ORDER BY Cle";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametres = await connection.QueryAsync<RepParametreEntity>(sql);
            return repParametres.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Parametre>> GetModifiablesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt
                FROM Parametres
                WHERE EstModifiable = 1
                ORDER BY Cle";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametres = await connection.QueryAsync<RepParametreEntity>(sql);
            return repParametres.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Parametre> AddAsync(Parametre parametre, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Parametres (Cle, Valeur, Description, Categorie, EstModifiable, CreatedAt, UpdatedAt)
                VALUES (@Cle, @Valeur, @Description, @Categorie, @EstModifiable, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametre = RepParametreEntity.FromDomain(parametre);
            var id = await connection.ExecuteScalarAsync<int>(sql, repParametre);
            return await GetByIdAsync(id, cancellationToken) ?? parametre;
        }

        public async Task UpdateAsync(Parametre parametre, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Parametres 
                SET 
                    Valeur = @Valeur,
                    Description = @Description,
                    Categorie = @Categorie,
                    EstModifiable = @EstModifiable,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repParametre = RepParametreEntity.FromDomain(parametre);
            var affectedRows = await connection.ExecuteAsync(sql, repParametre);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour le paramètre {ParametreId}", parametre.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Parametres WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Parametres WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> CleExistsAsync(string cle, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            string sql = "SELECT COUNT(1) FROM Parametres WHERE Cle = @Cle";
            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
            }

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Cle = cle, ExcludeId = excludeId });
            return count > 0;
        }

        // ============================================
        // UTILITAIRES
        // ============================================

        public async Task<T?> GetValueAsync<T>(string cle, T? defaultValue = default, CancellationToken cancellationToken = default)
        {
            var parametre = await GetByCleAsync(cle, cancellationToken);
            if (parametre == null || parametre.Valeur == null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(parametre.Valeur, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task<string?> GetValueAsync(string cle, CancellationToken cancellationToken = default)
        {
            var parametre = await GetByCleAsync(cle, cancellationToken);
            return parametre?.Valeur;
        }
    }
}