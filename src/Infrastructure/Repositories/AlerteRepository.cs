using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.Alertes.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    public class AlerteRepository : IAlerteRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<AlerteRepository> _logger;

        public AlerteRepository(DapperContext context, ILogger<AlerteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES - RECHERCHES DE BASE
        // ============================================

        public async Task<IEnumerable<Alerte>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql);
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<Alerte?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlerte = await connection.QueryFirstOrDefaultAsync<RepAlerteEntity>(sql, new { Id = id });
            return repAlerte?.ToDomain();
        }

        // ============================================
        // FILTRES PAR STATUT
        // ============================================

        public async Task<IEnumerable<Alerte>> GetNonLuesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.EstLue = 0
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql);
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Alerte>> GetLuesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.EstLue = 1
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql);
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // FILTRES PAR TYPE
        // ============================================

        public async Task<IEnumerable<Alerte>> GetByTypeAsync(TypeAlerte type, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.TypeAlerte = @TypeAlerte
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql, new { TypeAlerte = type.ToString() });
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Alerte>> GetByNiveauAsync(NiveauAlerte niveau, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.Niveau = @Niveau
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql, new { Niveau = niveau.ToString() });
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // FILTRES PAR ENTITÉ
        // ============================================

        public async Task<IEnumerable<Alerte>> GetByEntiteAsync(int entiteId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.EntiteId = @EntiteId
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql, new { EntiteId = entiteId });
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Alerte>> GetByEntiteAndTypeAsync(int entiteId, TypeAlerte type, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.EntiteId = @EntiteId AND a.TypeAlerte = @TypeAlerte
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql, new { EntiteId = entiteId, TypeAlerte = type.ToString() });
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        // ============================================
        // FILTRES PAR PÉRIODE
        // ============================================

        public async Task<IEnumerable<Alerte>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.*,
                    u.Nom as UtilisateurNom
                FROM Alertes a
                LEFT JOIN Utilisateurs u ON a.UtilisateurId = u.Id
                WHERE a.DateAlerte BETWEEN @DateDebut AND @DateFin
                ORDER BY a.DateAlerte DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlertes = await connection.QueryAsync<RepAlerteEntity>(sql, new { DateDebut = dateDebut, DateFin = dateFin });
            return repAlertes.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<Alerte>> GetLast24hAsync(CancellationToken cancellationToken = default)
        {
            var dateLimite = DateTime.UtcNow.AddHours(-24);
            return await GetByDateRangeAsync(dateLimite, DateTime.UtcNow, cancellationToken);
        }

        public async Task<IEnumerable<Alerte>> GetLast7DaysAsync(CancellationToken cancellationToken = default)
        {
            var dateLimite = DateTime.UtcNow.AddDays(-7);
            return await GetByDateRangeAsync(dateLimite, DateTime.UtcNow, cancellationToken);
        }

        // ============================================
        // STATISTIQUES
        // ============================================

        public async Task<int> CountNonLuesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Alertes WHERE EstLue = 0";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<Dictionary<TypeAlerte, int>> CountByTypeAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT TypeAlerte, COUNT(1) as Count FROM Alertes GROUP BY TypeAlerte";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var result = await connection.QueryAsync<(string TypeAlerte, int Count)>(sql);
            
            var dict = new Dictionary<TypeAlerte, int>();
            foreach (var item in result)
            {
                if (Enum.TryParse<TypeAlerte>(item.TypeAlerte, true, out var type))
                {
                    dict[type] = item.Count;
                }
            }
            return dict;
        }

        public async Task<Dictionary<NiveauAlerte, int>> CountByNiveauAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT Niveau, COUNT(1) as Count FROM Alertes GROUP BY Niveau";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var result = await connection.QueryAsync<(string Niveau, int Count)>(sql);
            
            var dict = new Dictionary<NiveauAlerte, int>();
            foreach (var item in result)
            {
                if (Enum.TryParse<NiveauAlerte>(item.Niveau, true, out var niveau))
                {
                    dict[niveau] = item.Count;
                }
            }
            return dict;
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<Alerte> AddAsync(Alerte alerte, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO Alertes (TypeAlerte, EntiteId, Niveau, Message, EstLue, DateAlerte, UtilisateurId)
                VALUES (@TypeAlerte, @EntiteId, @Niveau, @Message, @EstLue, @DateAlerte, @UtilisateurId);
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlerte = RepAlerteEntity.FromDomain(alerte);
            var id = await connection.ExecuteScalarAsync<int>(sql, repAlerte);
            return await GetByIdAsync(id, cancellationToken) ?? alerte;
        }

        public async Task UpdateAsync(Alerte alerte, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Alertes 
                SET 
                    EstLue = @EstLue,
                    DateLecture = @DateLecture,
                    UtilisateurId = @UtilisateurId
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var repAlerte = RepAlerteEntity.FromDomain(alerte);
            var affectedRows = await connection.ExecuteAsync(sql, repAlerte);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour l'alerte {AlerteId}", alerte.Id);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Alertes WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // ACTIONS SPÉCIALES
        // ============================================

        public async Task MarquerCommeLueAsync(int alerteId, int utilisateurId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Alertes 
                SET EstLue = 1, DateLecture = @DateLecture, UtilisateurId = @UtilisateurId
                WHERE Id = @AlerteId";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { AlerteId = alerteId, UtilisateurId = utilisateurId, DateLecture = DateTime.UtcNow });
        }

        public async Task MarquerPlusieursCommeLuesAsync(IEnumerable<int> alerteIds, int utilisateurId, CancellationToken cancellationToken = default)
        {
            var ids = string.Join(",", alerteIds);
            var sql = $@"
                UPDATE Alertes 
                SET EstLue = 1, DateLecture = @DateLecture, UtilisateurId = @UtilisateurId
                WHERE Id IN ({ids})";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { UtilisateurId = utilisateurId, DateLecture = DateTime.UtcNow });
        }

        public async Task MarquerToutesCommeLuesAsync(int utilisateurId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE Alertes 
                SET EstLue = 1, DateLecture = @DateLecture, UtilisateurId = @UtilisateurId
                WHERE EstLue = 0";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { UtilisateurId = utilisateurId, DateLecture = DateTime.UtcNow });
        }

        public async Task DeleteOldAsync(DateTime dateLimite, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Alertes WHERE DateAlerte < @DateLimite";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var deleted = await connection.ExecuteAsync(sql, new { DateLimite = dateLimite });
            _logger.LogInformation("{Count} alertes supprimées (plus anciennes que {DateLimite})", deleted, dateLimite);
        }

        // ============================================
        // VÉRIFICATIONS
        // ============================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM Alertes WHERE Id = @Id";
            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        // ============================================
        // GÉNÉRATION D'ALERTES MÉTIER (à implémenter avec les autres repositories)
        // ============================================

        public async Task CheckAndCreateStockFaibleAlertesAsync(CancellationToken cancellationToken = default)
        {
            // TODO: À implémenter avec IStockRepository
            // Vérifier les stocks en dessous du seuil d'alerte
            // Créer des alertes pour chaque stock faible
            await Task.CompletedTask;
        }

        public async Task CheckAndCreatePeremptionAlertesAsync(CancellationToken cancellationToken = default)
        {
            // TODO: À implémenter avec IStockRepository
            // Vérifier les stocks proches de la péremption
            // Créer des alertes pour chaque produit bientôt périmé
            await Task.CompletedTask;
        }

        public async Task CheckAndCreateBandeACloturerAlertesAsync(CancellationToken cancellationToken = default)
        {
            // TODO: À implémenter avec IBandeRepository
            // Vérifier les bandes proches de la fin du cycle
            // Créer des alertes pour chaque bande à clôturer
            await Task.CompletedTask;
        }
    }
}