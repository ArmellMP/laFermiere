using Dapper;
using Microsoft.Extensions.Logging;
using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    public class UtilisateurRepository : IUtilisateurRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<UtilisateurRepository> _logger;

        public UtilisateurRepository(DapperContext context, ILogger<UtilisateurRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Utilisateur>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Nom, Email, PasswordHash, Role, EstActif, 
                       DerniereConnexion, CreatedAt, UpdatedAt
                FROM Utilisateurs
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateurs = await connection.QueryAsync<RepUtilisateurEntity>(sql);
            return repUtilisateurs.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Utilisateur?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Nom, Email, PasswordHash, Role, EstActif, 
                       DerniereConnexion, CreatedAt, UpdatedAt
                FROM Utilisateurs
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateur = await connection.QueryFirstOrDefaultAsync<RepUtilisateurEntity>(sql, new { Id = id });
            return repUtilisateur?.ToDomain();
        }

        public async Task<IEnumerable<Utilisateur>> GetByRoleAsync(RoleUtilisateur role, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Nom, Email, PasswordHash, Role, EstActif, 
                       DerniereConnexion, CreatedAt, UpdatedAt
                FROM Utilisateurs
                WHERE Role = @Role
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateurs = await connection.QueryAsync<RepUtilisateurEntity>(sql, new { Role = role.ToString() });
            return repUtilisateurs.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Utilisateur>> GetActifsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Nom, Email, PasswordHash, Role, EstActif, 
                       DerniereConnexion, CreatedAt, UpdatedAt
                FROM Utilisateurs
                WHERE EstActif = 1
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateurs = await connection.QueryAsync<RepUtilisateurEntity>(sql);
            return repUtilisateurs.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Utilisateur> AddAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Utilisateurs (
                    Nom, Email, PasswordHash, Role, EstActif, 
                    DerniereConnexion, CreatedAt, UpdatedAt
                ) VALUES (
                    @Nom, @Email, @PasswordHash, @Role, @EstActif, 
                    @DerniereConnexion, @CreatedAt, @UpdatedAt
                );
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateur = RepUtilisateurEntity.FromDomain(utilisateur);
            var id = await connection.ExecuteScalarAsync<int>(sql, repUtilisateur);
            return await GetByIdAsync(id, cancellationToken) ?? utilisateur;
        }

        public async Task UpdateAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Utilisateurs 
                SET 
                    Nom = @Nom,
                    Email = @Email,
                    PasswordHash = @PasswordHash,
                    Role = @Role,
                    EstActif = @EstActif,
                    DerniereConnexion = @DerniereConnexion,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateur = RepUtilisateurEntity.FromDomain(utilisateur);
            var affectedRows = await connection.ExecuteAsync(sql, repUtilisateur);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour l'utilisateur {UtilisateurId}", utilisateur.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Utilisateurs WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task UpdateDerniereConnexionAsync(int id, DateTime dateConnexion, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Utilisateurs 
                SET DerniereConnexion = @DateConnexion
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id, DateConnexion = dateConnexion });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Utilisateurs WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            string sql = "SELECT COUNT(1) FROM Utilisateurs WHERE Email = @Email";
            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
            }

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<Utilisateur?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, Nom, Email, PasswordHash, Role, EstActif, 
                       DerniereConnexion, CreatedAt, UpdatedAt
                FROM Utilisateurs
                WHERE Email = @Email";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repUtilisateur = await connection.QueryFirstOrDefaultAsync<RepUtilisateurEntity>(sql, new { Email = email });
            
            if (repUtilisateur == null) return null;
            
            var utilisateur = repUtilisateur.ToDomain();
            
            // Vérifier le mot de passe
            if (!PasswordHasher.VerifyPassword(password, utilisateur.PasswordHash))
                return null;
            
            // Vérifier que l'utilisateur est actif
            if (!utilisateur.EstActif)
                return null;
            
            return utilisateur;
        }
    }
}