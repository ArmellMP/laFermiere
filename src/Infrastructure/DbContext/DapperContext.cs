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

        /// <summary>
        /// Active le mode strict de MySQL/MariaDB pour la session (la connexion en cours).
        ///
        /// Sans mode strict, le serveur "corrige" en silence une valeur invalide :
        ///   - un nom d'enum inconnu devient une chaîne vide ''
        ///   - un texte trop long est tronqué
        ///   - un nombre hors limites est ramené à la valeur maximale
        /// Les données sont alors faussées sans qu'aucune erreur ne remonte.
        ///
        /// Avec STRICT_ALL_TABLES, ces cas provoquent une erreur SQL : l'opération est
        /// refusée et l'ExceptionMiddleware renvoie une erreur au lieu de corrompre la base.
        ///
        /// CONCAT_WS conserve les options déjà actives (NO_ZERO_DATE, etc.) et ajoute
        /// seulement STRICT_ALL_TABLES. Le réglage serveur (XAMPP) n'est pas modifié.
        /// </summary>
        private const string ActiverModeStrict =
            "SET SESSION sql_mode = CONCAT_WS(',', @@SESSION.sql_mode, 'STRICT_ALL_TABLES')";

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
            // À refaire à chaque ouverture : une connexion réutilisée depuis le pool
            // est réinitialisée et perd les réglages de session précédents.
            connection.Execute(ActiverModeStrict);
            return connection;
        }

        /// <summary>
        /// Crée une connexion asynchrone.
        /// </summary>
        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            // Même réglage que CreateConnection (voir ActiverModeStrict)
            await connection.ExecuteAsync(new CommandDefinition(ActiverModeStrict, cancellationToken: cancellationToken));
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