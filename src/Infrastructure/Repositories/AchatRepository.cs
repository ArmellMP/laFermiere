using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    public class AchatRepository : IAchatRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<AchatRepository> _logger;

        public AchatRepository(DapperContext context, ILogger<AchatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<Achat>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                ORDER BY a.DateAchat DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchats = await connection.QueryAsync<RepAchatEntity>(sql);
            return repAchats.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Achat?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                WHERE a.Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchat = await connection.QueryFirstOrDefaultAsync<RepAchatEntity>(sql, new { Id = id });
            return repAchat?.ToDomain();
        }

        public async Task<IEnumerable<Achat>> GetByBandeIdAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                WHERE a.BandeId = @BandeId
                ORDER BY a.DateAchat DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchats = await connection.QueryAsync<RepAchatEntity>(sql, new { BandeId = bandeId });
            return repAchats.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Achat>> GetByFournisseurIdAsync(int fournisseurId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                WHERE a.FournisseurId = @FournisseurId
                ORDER BY a.DateAchat DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchats = await connection.QueryAsync<RepAchatEntity>(sql, new { FournisseurId = fournisseurId });
            return repAchats.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Achat>> GetByTypeAsync(TypeAchat type, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                WHERE a.TypeAchat = @TypeAchat
                ORDER BY a.DateAchat DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchats = await connection.QueryAsync<RepAchatEntity>(sql, new { TypeAchat = type.ToString() });
            return repAchats.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Achat>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                WHERE a.DateAchat BETWEEN @DateDebut AND @DateFin
                ORDER BY a.DateAchat DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchats = await connection.QueryAsync<RepAchatEntity>(sql, new { DateDebut = dateDebut, DateFin = dateFin });
            return repAchats.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Achat>> GetNonRecusAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    f.Nom as FournisseurNom,
                    b.Code as BandeCode,
                    u.Nom as CreateurNom
                FROM Achats a
                LEFT JOIN Fournisseurs f ON a.FournisseurId = f.Id
                LEFT JOIN Bandes b ON a.BandeId = b.Id
                LEFT JOIN Utilisateurs u ON a.CreatedBy = u.Id
                WHERE a.DateReception IS NULL
                ORDER BY a.DateAchat DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchats = await connection.QueryAsync<RepAchatEntity>(sql);
            return repAchats.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<decimal> GetTotalAchatsByBandeAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COALESCE(SUM(MontantTotal), 0) FROM Achats WHERE BandeId = @BandeId";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var total = await connection.ExecuteScalarAsync<decimal>(sql, new { BandeId = bandeId });
            return total;
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Achat> AddAsync(Achat achat, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Achats (
                    FournisseurId, BandeId, DateAchat, TypeAchat, Description,
                    Quantite, UniteMesure, PrixUnitaire, MontantTotal,
                    FactureNumero, DateReception, Observations, CreatedAt, CreatedBy
                ) VALUES (
                    @FournisseurId, @BandeId, @DateAchat, @TypeAchat, @Description,
                    @Quantite, @UniteMesure, @PrixUnitaire, @MontantTotal,
                    @FactureNumero, @DateReception, @Observations, @CreatedAt, @CreatedBy
                );
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchat = RepAchatEntity.FromDomain(achat);
            var id = await connection.ExecuteScalarAsync<int>(sql, repAchat);
            return await GetByIdAsync(id, cancellationToken) ?? achat;
        }

        public async Task UpdateAsync(Achat achat, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Achats 
                SET 
                    DateAchat = @DateAchat,
                    TypeAchat = @TypeAchat,
                    Description = @Description,
                    Quantite = @Quantite,
                    UniteMesure = @UniteMesure,
                    PrixUnitaire = @PrixUnitaire,
                    MontantTotal = @MontantTotal,
                    FactureNumero = @FactureNumero,
                    DateReception = @DateReception,
                    Observations = @Observations
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAchat = RepAchatEntity.FromDomain(achat);
            var affectedRows = await connection.ExecuteAsync(sql, repAchat);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour l'achat {AchatId}", achat.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Achats WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Achats WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> HasAchatsByFournisseurAsync(int fournisseurId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Achats WHERE FournisseurId = @FournisseurId";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { FournisseurId = fournisseurId });
            return count > 0;
        }

        public async Task<bool> HasAchatsByBandeAsync(int bandeId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Achats WHERE BandeId = @BandeId";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BandeId = bandeId });
            return count > 0;
        }
    }
}