using Dapper;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Entities;
using Infrastructure.DbContext.DapperContext;

namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DapperContext _context;

        public RefreshTokenRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO RefreshTokens (UtilisateurId, TokenHash, FamilleId, ExpireLe, CreatedAt)
                VALUES (@UtilisateurId, @TokenHash, @FamilleId, @ExpireLe, @CreatedAt);
                SELECT LAST_INSERT_ID();";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            refreshToken.Id = await connection.ExecuteScalarAsync<int>(sql, refreshToken);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT Id, UtilisateurId, TokenHash, FamilleId, ExpireLe, UtiliseLe, RevoqueLe, CreatedAt
                FROM RefreshTokens
                WHERE TokenHash = @TokenHash";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { TokenHash = tokenHash });
        }

        public async Task<bool> MarquerUtiliseAsync(int id, DateTime dateUtc, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE RefreshTokens
                SET UtiliseLe = @Date
                WHERE Id = @Id AND UtiliseLe IS NULL AND RevoqueLe IS NULL";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            return await connection.ExecuteAsync(sql, new { Id = id, Date = dateUtc }) == 1;
        }

        public async Task RevoquerFamilleAsync(string familleId, DateTime dateUtc, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE RefreshTokens
                SET RevoqueLe = @Date
                WHERE FamilleId = @FamilleId AND RevoqueLe IS NULL";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { FamilleId = familleId, Date = dateUtc });
        }

        public async Task RevoquerToutPourUtilisateurAsync(int utilisateurId, DateTime dateUtc, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE RefreshTokens
                SET RevoqueLe = @Date
                WHERE UtilisateurId = @UtilisateurId AND RevoqueLe IS NULL";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { UtilisateurId = utilisateurId, Date = dateUtc });
        }

        public async Task SupprimerExpiresAsync(int utilisateurId, DateTime maintenantUtc, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM RefreshTokens WHERE UtilisateurId = @UtilisateurId AND ExpireLe < @Maintenant";

            using var connection = await _context.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, new { UtilisateurId = utilisateurId, Maintenant = maintenantUtc });
        }
    }
}
