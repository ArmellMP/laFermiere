using Dapper;
using Microsoft.Extensions.Logging;
using Application.Features.SuivisJournaliers.Interfaces;
using Domain.Entities;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.RepEntities;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation de ISuiviJournalierRepository avec Dapper.
    /// Contient toutes les opérations SQL pour la table SuivisJournaliers.
    /// </summary>
    public class SuiviJournalierRepository : ISuiviJournalierRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<SuiviJournalierRepository> _logger;

        public SuiviJournalierRepository(DapperContext context, ILogger<SuiviJournalierRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LECTURES
        // ============================================

        public async Task<IEnumerable<SuiviJournalier>> GetByFiltresAsync(
            int? bandeId,
            DateTime? dateDebut,
            DateTime? dateFin,
            CancellationToken cancellationToken = default)
        {
            // Chaque filtre suit le motif "(@Param IS NULL OR condition)" :
            // si le paramètre vaut NULL, la condition est toujours vraie => filtre ignoré.
            // Une seule requête couvre ainsi toutes les combinaisons de filtres,
            // sans construire de SQL dynamique (et sans risque d'injection SQL).
            const string sql = @"
                SELECT
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE (@BandeId IS NULL OR sj.BandeId = @BandeId)
                  AND (@DateDebut IS NULL OR sj.DateSuivi >= @DateDebut)
                  AND (@DateFin IS NULL OR sj.DateSuivi <= @DateFin)
                ORDER BY sj.BandeId ASC, sj.Jour ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);

            var repSuivis = await connection.QueryAsync<RepSuiviJournalierEntity>(sql, new
            {
                BandeId = bandeId,
                DateDebut = dateDebut,
                DateFin = dateFin
            });

            return repSuivis.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<SuiviJournalier>> GetByBandeIdAsync(
            int bandeId, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.BandeId = @BandeId
                ORDER BY sj.Jour ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivis = await connection.QueryAsync<RepSuiviJournalierEntity>(sql, new { BandeId = bandeId });
            
            return repSuivis.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<SuiviJournalier?> GetByIdAsync(
            int id, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivi = await connection.QueryFirstOrDefaultAsync<RepSuiviJournalierEntity>(sql, new { Id = id });
            
            return repSuivi?.ToDomain();
        }

        public async Task<SuiviJournalier?> GetLastByBandeIdAsync(
            int bandeId, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.BandeId = @BandeId
                ORDER BY sj.Jour DESC
                LIMIT 1";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivi = await connection.QueryFirstOrDefaultAsync<RepSuiviJournalierEntity>(sql, new { BandeId = bandeId });
            
            return repSuivi?.ToDomain();
        }

        public async Task<IEnumerable<SuiviJournalier>> GetByBandeIdAndDateRangeAsync(
            int bandeId, 
            DateTime dateDebut, 
            DateTime dateFin, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.BandeId = @BandeId
                  AND sj.DateSuivi BETWEEN @DateDebut AND @DateFin
                ORDER BY sj.Jour ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivis = await connection.QueryAsync<RepSuiviJournalierEntity>(sql, new 
            { 
                BandeId = bandeId, 
                DateDebut = dateDebut, 
                DateFin = dateFin 
            });
            
            return repSuivis.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<bool> ExistsForDateAsync(
            int bandeId, 
            DateTime dateSuivi, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM SuivisJournaliers 
                WHERE BandeId = @BandeId AND DateSuivi = @DateSuivi";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BandeId = bandeId, DateSuivi = dateSuivi });
            
            return count > 0;
        }

        // ============================================
        // ÉCRITURES
        // ============================================

        public async Task<SuiviJournalier> AddAsync(
            SuiviJournalier suivi, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO SuivisJournaliers (
                    BandeId, Jour, DateSuivi, NbMortalites, CauseMortalite,
                    PoidsMoyen, NbSujetsPeses, TypeSoin, ProduitUtilise, Dosage,
                    DureeTraitement, CoutSoin, StockConsommeId, QuantiteConsommee,
                    Temperature, Observations, CreatedAt, CreatedBy
                ) VALUES (
                    @BandeId, @Jour, @DateSuivi, @NbMortalites, @CauseMortalite,
                    @PoidsMoyen, @NbSujetsPeses, @TypeSoin, @ProduitUtilise, @Dosage,
                    @DureeTraitement, @CoutSoin, @StockConsommeId, @QuantiteConsommee,
                    @Temperature, @Observations, @CreatedAt, @CreatedBy
                );
                
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivi = RepSuiviJournalierEntity.FromDomain(suivi);
            
            var id = await connection.ExecuteScalarAsync<int>(sql, repSuivi);
            
            return await GetByIdAsync(id, cancellationToken) ?? suivi;
        }

        public async Task UpdateAsync(
            SuiviJournalier suivi, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE SuivisJournaliers 
                SET 
                    NbMortalites = @NbMortalites,
                    CauseMortalite = @CauseMortalite,
                    PoidsMoyen = @PoidsMoyen,
                    NbSujetsPeses = @NbSujetsPeses,
                    TypeSoin = @TypeSoin,
                    ProduitUtilise = @ProduitUtilise,
                    Dosage = @Dosage,
                    DureeTraitement = @DureeTraitement,
                    CoutSoin = @CoutSoin,
                    StockConsommeId = @StockConsommeId,
                    QuantiteConsommee = @QuantiteConsommee,
                    Temperature = @Temperature,
                    Observations = @Observations,
                    UpdatedAt = NOW()
                WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivi = RepSuiviJournalierEntity.FromDomain(suivi);
            
            var affectedRows = await connection.ExecuteAsync(sql, repSuivi);
            
            if (affectedRows == 0)
            {
                _logger.LogWarning("Aucune ligne mise à jour pour le suivi {SuiviId}", suivi.Id);
            }
        }

        public async Task DeleteAsync(
            int id, 
            CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM SuivisJournaliers WHERE Id = @Id";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // ============================================
        // MÉTHODES SUPPLÉMENTAIRES (AGRÉGATIONS ET STATISTIQUES)
        // ============================================

        public async Task<IEnumerable<SuiviJournalier>> GetWithAbnormalTemperatureAsync(
            decimal? minTemp = null, 
            decimal? maxTemp = null, 
            CancellationToken cancellationToken = default)
        {
            // Valeurs par défaut si non spécifiées (température idéale 18-25°C)
            var min = minTemp ?? 18;
            var max = maxTemp ?? 25;
            
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.Temperature IS NOT NULL 
                  AND (sj.Temperature < @MinTemp OR sj.Temperature > @MaxTemp)
                ORDER BY sj.Temperature DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivis = await connection.QueryAsync<RepSuiviJournalierEntity>(
                sql, new { MinTemp = min, MaxTemp = max });
            
            return repSuivis.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<IEnumerable<SuiviJournalier>> GetWithHighMortalityAsync(
            int seuilMortalite = 5, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.NbMortalites >= @Seuil
                ORDER BY sj.NbMortalites DESC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivis = await connection.QueryAsync<RepSuiviJournalierEntity>(
                sql, new { Seuil = seuilMortalite });
            
            return repSuivis.Select(rep => rep.ToDomain()).ToList();
        }

        public async Task<decimal?> GetAveragePoidsByBandeIdAndDateRangeAsync(
            int bandeId, 
            DateTime dateDebut, 
            DateTime dateFin, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT AVG(PoidsMoyen) 
                FROM SuivisJournaliers 
                WHERE BandeId = @BandeId 
                  AND DateSuivi BETWEEN @DateDebut AND @DateFin
                  AND PoidsMoyen IS NOT NULL";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var avg = await connection.ExecuteScalarAsync<decimal?>(
                sql, new { BandeId = bandeId, DateDebut = dateDebut, DateFin = dateFin });
            
            return avg;
        }

        public async Task<int> GetTotalMortalitesByBandeIdAndDateRangeAsync(
            int bandeId, 
            DateTime dateDebut, 
            DateTime dateFin, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COALESCE(SUM(NbMortalites), 0)
                FROM SuivisJournaliers 
                WHERE BandeId = @BandeId 
                  AND DateSuivi BETWEEN @DateDebut AND @DateFin";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var total = await connection.ExecuteScalarAsync<int>(
                sql, new { BandeId = bandeId, DateDebut = dateDebut, DateFin = dateFin });
            
            return total;
        }

        public async Task<IEnumerable<SuiviJournalier>> GetWithSoinsAsync(
            int bandeId, 
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    sj.*,
                    b.Code as BandeCode,
                    s.Nom as StockNom,
                    s.UniteMesure as StockUniteMesure,
                    u.Nom as CreateurNom
                FROM SuivisJournaliers sj
                LEFT JOIN Bandes b ON sj.BandeId = b.Id
                LEFT JOIN Stocks s ON sj.StockConsommeId = s.Id
                LEFT JOIN Utilisateurs u ON sj.CreatedBy = u.Id
                WHERE sj.BandeId = @BandeId
                  AND sj.TypeSoin IS NOT NULL
                ORDER BY sj.Jour ASC";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            
            var repSuivis = await connection.QueryAsync<RepSuiviJournalierEntity>(
                sql, new { BandeId = bandeId });
            
            return repSuivis.Select(rep => rep.ToDomain()).ToList();
        }
    }
}