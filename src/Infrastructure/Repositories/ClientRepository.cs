using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Clients.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation de IClientRepository avec Dapper.
    /// </summary>
    public class ClientRepository : IClientRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<ClientRepository> _logger;

        public ClientRepository(DapperContext context, ILogger<ClientRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeClient, Telephone, Email, Adresse, Notes,
                    EstActif, CreatedAt, UpdatedAt
                FROM Clients
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClients = await connection.QueryAsync<RepClientEntity>(sql);
            
            return repClients.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeClient, Telephone, Email, Adresse, Notes,
                    EstActif, CreatedAt, UpdatedAt
                FROM Clients
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClient = await connection.QueryFirstOrDefaultAsync<RepClientEntity>(sql, new { Id = id });
            
            return repClient?.ToDomain();
        }

        public async Task<IEnumerable<Client>> GetByNomAsync(string nom, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeClient, Telephone, Email, Adresse, Notes,
                    EstActif, CreatedAt, UpdatedAt
                FROM Clients
                WHERE Nom LIKE @Nom
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClients = await connection.QueryAsync<RepClientEntity>(sql, new { Nom = $"%{nom}%" });
            
            return repClients.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Client>> GetByTypeAsync(TypeClient type, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeClient, Telephone, Email, Adresse, Notes,
                    EstActif, CreatedAt, UpdatedAt
                FROM Clients
                WHERE TypeClient = @TypeClient
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClients = await connection.QueryAsync<RepClientEntity>(sql, new { TypeClient = type.ToString() });
            
            return repClients.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Client>> GetActifsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Nom, TypeClient, Telephone, Email, Adresse, Notes,
                    EstActif, CreatedAt, UpdatedAt
                FROM Clients
                WHERE EstActif = 1
                ORDER BY Nom ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClients = await connection.QueryAsync<RepClientEntity>(sql);
            
            return repClients.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Clients (
                    Nom, TypeClient, Telephone, Email, Adresse, Notes,
                    EstActif, CreatedAt, UpdatedAt
                ) VALUES (
                    @Nom, @TypeClient, @Telephone, @Email, @Adresse, @Notes,
                    @EstActif, @CreatedAt, @UpdatedAt
                );
                
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClient = RepClientEntity.FromDomain(client);
            
            var id = await connection.ExecuteScalarAsync<int>(sql, repClient);
            
            return await GetByIdAsync(id, cancellationToken) ?? client;
        }

        public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Clients 
                SET 
                    Nom = @Nom,
                    TypeClient = @TypeClient,
                    Telephone = @Telephone,
                    Email = @Email,
                    Adresse = @Adresse,
                    Notes = @Notes,
                    EstActif = @EstActif,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repClient = RepClientEntity.FromDomain(client);
            
            var affectedRows = await connection.ExecuteAsync(sql, repClient);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour le client {ClientId}", client.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Clients WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Clients WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            string sql = "SELECT COUNT(1) FROM Clients WHERE Email = @Email";
            
            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
            }

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, ExcludeId = excludeId });
            
            return count > 0;
        }

        public async Task<bool> HasVentesAsync(int clientId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Ventes WHERE ClientId = @ClientId";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { ClientId = clientId });
            
            return count > 0;
        }
    }
}