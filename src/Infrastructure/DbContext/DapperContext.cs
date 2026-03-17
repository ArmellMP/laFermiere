using System.Data;
using Dapper; 
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Infrastructure.DbContext.DapperContext
{
    /// <summary>
    /// Contexte Dapper pour la connexion à MySQL.
    /// Gère la création et l'ouverture des connexions.
    /// </summary>
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// Crée et ouvre une connexion IDbConnection.
        /// </summary>
        public IDbConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Crée une connexion asynchrone.
        /// </summary>
        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        /// <summary>
        /// Teste la connexion à la base de données.
        /// </summary>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = await CreateConnectionAsync(cancellationToken);
                var result = await connection.ExecuteScalarAsync<string>("SELECT 'OK'");
                return result == "OK";
            }
            catch
            {
                return false;
            }
        }
    }
}