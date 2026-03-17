using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using Dapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using System.Data;

namespace Infrastructure.Repositories.BandeRepository
{
    /// <summary>
    /// Implémentation de IBandeRepository avec Dapper.
    /// Contient toutes les opérations SQL pour la table Bandes.
    /// </summary>
    public class BandeRepository : IBandeRepository
    {
        private readonly DapperContext _context;

        public BandeRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bande>> GetAllAsync(CancellationToken cancellationToken = default)
        { 
            const string sql = @"
                SELECT 
                    Id, Code, DateArrivee, NbPoussinsInit, NbPoussinsActuel,
                    Statut, DateCloture, Observations, CreatedAt, UpdatedAt
                FROM Bandes
                ORDER BY DateArrivee DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var bandes = await connection.QueryAsync<Bande>(sql);
            return bandes;
        }

        public async Task<Bande?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Code, DateArrivee, NbPoussinsInit, NbPoussinsActuel,
                    Statut, DateCloture, Observations, CreatedAt, UpdatedAt
                FROM Bandes 
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var bande = await connection.QueryFirstOrDefaultAsync<Bande>(sql, new { Id = id });
            return bande;
        }

        public async Task<Bande?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Code, DateArrivee, NbPoussinsInit, NbPoussinsActuel,
                    Statut, DateCloture, Observations, CreatedAt, UpdatedAt
                FROM Bandes 
                WHERE Code = @Code";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var bande = await connection.QueryFirstOrDefaultAsync<Bande>(sql, new { Code = code });
            return bande;
        }

        public async Task<IEnumerable<Bande>> GetByStatutAsync(StatutBande statut, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id, Code, DateArrivee, NbPoussinsInit, NbPoussinsActuel,
                    Statut, DateCloture, Observations, CreatedAt, UpdatedAt
                FROM Bandes 
                WHERE Statut = @Statut
                ORDER BY DateArrivee DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var bandes = await connection.QueryAsync<Bande>(sql, new { Statut = statut });
            return bandes;
        }

        public async Task<Bande> AddAsync(Bande bande, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Bandes 
                    (Code, DateArrivee, NbPoussinsInit, NbPoussinsActuel, 
                     Statut, DateCloture, Observations, CreatedAt, UpdatedAt)
                VALUES 
                    (@Code, @DateArrivee, @NbPoussinsInit, @NbPoussinsActuel,
                     @Statut, @DateCloture, @Observations, @CreatedAt, @UpdatedAt);
                
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            // Exécuter l'INSERT et récupérer l'ID généré
            var bandeId = await connection.ExecuteScalarAsync<int>(sql, bande);
            
            var createdBande = await GetByIdAsync(bandeId, cancellationToken);
            
            return createdBande ?? bande; // Fallback si problème
        }

        public async Task UpdateAsync(Bande bande, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Bandes 
                SET 
                    Code = @Code,
                    DateArrivee = @DateArrivee,
                    NbPoussinsInit = @NbPoussinsInit,
                    NbPoussinsActuel = @NbPoussinsActuel,
                    Statut = @Statut,
                    DateCloture = @DateCloture,
                    Observations = @Observations,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, bande);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Bandes WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Bandes WHERE Code = @Code";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code });
            return count > 0;
        }

        public async Task<BandeStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalBandes,
                    SUM(CASE WHEN Statut = 'EnPreparation' THEN 1 ELSE 0 END) as BandesEnPreparation,
                    SUM(CASE WHEN Statut = 'EnCours' THEN 1 ELSE 0 END) as BandesEnCours,
                    SUM(CASE WHEN Statut = 'Cloturee' THEN 1 ELSE 0 END) as BandesCloturees,
                    SUM(CASE WHEN Statut = 'Archivee' THEN 1 ELSE 0 END) as BandesArchivees
                FROM Bandes";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var stats = await connection.QueryFirstOrDefaultAsync<BandeStatsDto>(sql);
            
            // Si pas de données, retourner un objet vide
            return stats ?? new BandeStatsDto();
        }
    }
}