using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Ventes.Interfaces;
using Domain.Entities;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    public class VenteRepository : IVenteRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<VenteRepository> _logger;

        public VenteRepository(DapperContext context, ILogger<VenteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Vente>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    v.*,
                    b.Code as BandeCode,
                    c.Nom as ClientNom,
                    u.Nom as CreateurNom
                FROM Ventes v
                LEFT JOIN Bandes b ON v.BandeId = b.Id
                LEFT JOIN Clients c ON v.ClientId = c.Id
                LEFT JOIN Utilisateurs u ON v.CreatedBy = u.Id
                ORDER BY v.DateVente DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVentes = await connection.QueryAsync<RepVenteEntity>(sql);
            return repVentes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Vente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    v.*,
                    b.Code as BandeCode,
                    c.Nom as ClientNom,
                    u.Nom as CreateurNom
                FROM Ventes v
                LEFT JOIN Bandes b ON v.BandeId = b.Id
                LEFT JOIN Clients c ON v.ClientId = c.Id
                LEFT JOIN Utilisateurs u ON v.CreatedBy = u.Id
                WHERE v.Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVente = await connection.QueryFirstOrDefaultAsync<RepVenteEntity>(sql, new { Id = id });
            return repVente?.ToDomain();
        }

        public async Task<IEnumerable<Vente>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    v.*,
                    b.Code as BandeCode,
                    c.Nom as ClientNom,
                    u.Nom as CreateurNom
                FROM Ventes v
                LEFT JOIN Bandes b ON v.BandeId = b.Id
                LEFT JOIN Clients c ON v.ClientId = c.Id
                LEFT JOIN Utilisateurs u ON v.CreatedBy = u.Id
                WHERE v.BandeId = @BandeId
                ORDER BY v.DateVente DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVentes = await connection.QueryAsync<RepVenteEntity>(sql, new { BandeId = bandeId });
            return repVentes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Vente>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    v.*,
                    b.Code as BandeCode,
                    c.Nom as ClientNom,
                    u.Nom as CreateurNom
                FROM Ventes v
                LEFT JOIN Bandes b ON v.BandeId = b.Id
                LEFT JOIN Clients c ON v.ClientId = c.Id
                LEFT JOIN Utilisateurs u ON v.CreatedBy = u.Id
                WHERE v.ClientId = @ClientId
                ORDER BY v.DateVente DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVentes = await connection.QueryAsync<RepVenteEntity>(sql, new { ClientId = clientId });
            return repVentes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Vente>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    v.*,
                    b.Code as BandeCode,
                    c.Nom as ClientNom,
                    u.Nom as CreateurNom
                FROM Ventes v
                LEFT JOIN Bandes b ON v.BandeId = b.Id
                LEFT JOIN Clients c ON v.ClientId = c.Id
                LEFT JOIN Utilisateurs u ON v.CreatedBy = u.Id
                WHERE v.DateVente BETWEEN @DateDebut AND @DateFin
                ORDER BY v.DateVente DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVentes = await connection.QueryAsync<RepVenteEntity>(sql, new { DateDebut = dateDebut, DateFin = dateFin });
            return repVentes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<decimal> GetTotalVentesByBandeAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COALESCE(SUM(MontantTotal), 0)
                FROM Ventes
                WHERE BandeId = @BandeId";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var total = await connection.ExecuteScalarAsync<decimal>(sql, new { BandeId = bandeId });
            return total;
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Vente> AddAsync(Vente vente, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Ventes (
                    BandeId, ClientId, DateVente, TypeVente, Quantite,
                    UniteMesure, PrixUnitaire, MontantTotal, ModePaiement,
                    EstPaye, DatePaiement, Observations, CreatedAt, CreatedBy
                ) VALUES (
                    @BandeId, @ClientId, @DateVente, @TypeVente, @Quantite,
                    @UniteMesure, @PrixUnitaire, @MontantTotal, @ModePaiement,
                    @EstPaye, @DatePaiement, @Observations, @CreatedAt, @CreatedBy
                );
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVente = RepVenteEntity.FromDomain(vente);
            var id = await connection.ExecuteScalarAsync<int>(sql, repVente);
            return await GetByIdAsync(id, cancellationToken) ?? vente;
        }

        public async Task UpdateAsync(Vente vente, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Ventes 
                SET 
                    DateVente = @DateVente,
                    TypeVente = @TypeVente,
                    Quantite = @Quantite,
                    UniteMesure = @UniteMesure,
                    PrixUnitaire = @PrixUnitaire,
                    MontantTotal = @MontantTotal,
                    ModePaiement = @ModePaiement,
                    EstPaye = @EstPaye,
                    DatePaiement = @DatePaiement,
                    Observations = @Observations
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repVente = RepVenteEntity.FromDomain(vente);
            var affectedRows = await connection.ExecuteAsync(sql, repVente);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour la vente {VenteId}", vente.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Ventes WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Ventes WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> HasVentesByBandeAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Ventes WHERE BandeId = @BandeId";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BandeId = bandeId });
            return count > 0;
        }

        public async Task<bool> HasVentesByClientAsync(int clientId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Ventes WHERE ClientId = @ClientId";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { ClientId = clientId });
            return count > 0;
        }
    }
}