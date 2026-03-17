// Infrastructure/Repositories/StockRepository.cs
using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Stocks.Interfaces;
using Domain.Entities;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;
using System.Data;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation de IStockRepository avec Dapper.
    /// Contient toutes les opérations SQL pour la table Stocks.
    /// </summary>
    public class StockRepository : IStockRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<StockRepository> _logger;

        public StockRepository(DapperContext context, ILogger<StockRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    s.*,
                    b.Code as BandeCode,
                    f.Nom as FournisseurNom
                FROM Stocks s
                LEFT JOIN Bandes b ON s.BandeId = b.Id
                LEFT JOIN Fournisseurs f ON s.FournisseurId = f.Id
                ORDER BY s.CreatedAt DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repStocks = await connection.QueryAsync<RepStockEntity>(sql);
            
            return repStocks.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Stock>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    s.*,
                    b.Code as BandeCode,
                    f.Nom as FournisseurNom
                FROM Stocks s
                LEFT JOIN Bandes b ON s.BandeId = b.Id
                LEFT JOIN Fournisseurs f ON s.FournisseurId = f.Id
                WHERE s.BandeId = @BandeId
                ORDER BY s.Type, s.Nom";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repStocks = await connection.QueryAsync<RepStockEntity>(sql, new { BandeId = bandeId });
            
            return repStocks.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    s.*,
                    b.Code as BandeCode,
                    f.Nom as FournisseurNom
                FROM Stocks s
                LEFT JOIN Bandes b ON s.BandeId = b.Id
                LEFT JOIN Fournisseurs f ON s.FournisseurId = f.Id
                WHERE s.Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repStock = await connection.QueryFirstOrDefaultAsync<RepStockEntity>(sql, new { Id = id });
            
            return repStock?.ToDomain();
        }

        public async Task<IEnumerable<Stock>> GetEnAlerteAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    s.*,
                    b.Code as BandeCode,
                    f.Nom as FournisseurNom
                FROM Stocks s
                LEFT JOIN Bandes b ON s.BandeId = b.Id
                LEFT JOIN Fournisseurs f ON s.FournisseurId = f.Id
                WHERE s.QuantiteActuelle <= COALESCE(s.SeuilAlerte, 0)
                ORDER BY (s.QuantiteActuelle / s.QuantiteInitiale) ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repStocks = await connection.QueryAsync<RepStockEntity>(sql);
            
            return repStocks.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Stock>> GetPerimesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    s.*,
                    b.Code as BandeCode,
                    f.Nom as FournisseurNom
                FROM Stocks s
                LEFT JOIN Bandes b ON s.BandeId = b.Id
                LEFT JOIN Fournisseurs f ON s.FournisseurId = f.Id
                WHERE s.DatePeremption < CURDATE()
                ORDER BY s.DatePeremption";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repStocks = await connection.QueryAsync<RepStockEntity>(sql);
            
            return repStocks.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Stock> AddAsync(Stock stock, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Stocks (
                    BandeId, Type, Nom, QuantiteInitiale, QuantiteActuelle,
                    UniteMesure, SeuilAlerte, DatePeremption, PrixUnitaire,
                    FournisseurId, CreatedAt, UpdatedAt
                ) VALUES (
                    @BandeId, @Type, @Nom, @QuantiteInitiale, @QuantiteActuelle,
                    @UniteMesure, @SeuilAlerte, @DatePeremption, @PrixUnitaire,
                    @FournisseurId, @CreatedAt, @UpdatedAt
                );
                
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            // Convertir l'entité Domain en RepEntity
            var repStock = RepStockEntity.FromDomain(stock);
            
            // Exécuter l'insertion
            var id = await connection.ExecuteScalarAsync<int>(sql, repStock);
            
            // Récupérer le stock créé
            return await GetByIdAsync(id, cancellationToken) ?? stock;
        }

        public async Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Stocks 
                SET 
                    QuantiteActuelle = @QuantiteActuelle,
                    SeuilAlerte = @SeuilAlerte,
                    DatePeremption = @DatePeremption,
                    PrixUnitaire = @PrixUnitaire,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repStock = RepStockEntity.FromDomain(stock);
            
            var affectedRows = await connection.ExecuteAsync(sql, repStock);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour le stock {StockId}", stock.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Stocks WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Stocks WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            
            return count > 0;
        }

        public async Task<bool> HasConsommationAsync(int stockId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM SuivisJournaliers 
                WHERE StockConsommeId = @StockId";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { StockId = stockId });
            
            return count > 0;
        }

        public async Task<decimal> GetQuantiteDisponibleAsync(int stockId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT QuantiteActuelle FROM Stocks WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            return await connection.ExecuteScalarAsync<decimal>(sql, new { Id = stockId });
        }
    }
}