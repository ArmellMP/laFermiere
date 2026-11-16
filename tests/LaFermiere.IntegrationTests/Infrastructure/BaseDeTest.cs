using System.Text.RegularExpressions;
using Application.Common.Security;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace LaFermiere.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Base de données dédiée aux tests : lafermiere_tests.
    ///
    /// À chaque lancement des tests d'intégration :
    ///   1. elle est SUPPRIMÉE puis recréée à partir de database/schema.sql
    ///   2. on y ajoute un compte de test par rôle
    /// Les tests partent donc toujours d'une base propre et connue.
    ///
    /// La base de travail "lafermiere" n'est JAMAIS touchée : le nom de la base de test
    /// est imposé ici, et schema.sql est réécrit pour viser lafermiere_tests.
    ///
    /// Identifiants MySQL (jamais écrits dans le dépôt), dans cet ordre :
    ///   1. variable d'environnement LAFERMIERE_TEST_DB_CONNECTION (ex. sur un serveur d'intégration)
    ///   2. sinon, les user-secrets du projet PresentationApi (ConnectionStrings:DefaultConnection)
    /// </summary>
    public static class BaseDeTest
    {
        public const string NomBase = "lafermiere_tests";

        /// <summary>Chaîne de connexion de l'API pendant les tests : pointe sur lafermiere_tests.</summary>
        public static string ChaineDeConnexion()
        {
            var builder = new MySqlConnectionStringBuilder(ChaineDeBase()) { Database = NomBase };
            return builder.ConnectionString;
        }

        /// <summary>Supprime et recrée lafermiere_tests, puis ajoute les comptes de test.</summary>
        public static async Task RecreerAsync()
        {
            // Connexion au SERVEUR, sans base sélectionnée : le script fait lui-même
            // DROP DATABASE / CREATE DATABASE / USE.
            var serveur = new MySqlConnectionStringBuilder(ChaineDeBase()) { Database = "" };

            // schema.sql vise "lafermiere" : on remplace le nom de base par lafermiere_tests.
            // \b = limite de mot : "lafermiere" seul est remplacé, pas un mot plus long.
            var script = Regex.Replace(File.ReadAllText(CheminsDuDepot.SchemaSql()), @"\blafermiere\b", NomBase);

            // Garde-fou : après remplacement, plus aucune instruction ne doit viser la base de travail
            if (Regex.IsMatch(script, @"(DROP|CREATE|USE)\s+(DATABASE\s+(IF\s+EXISTS\s+)?)?lafermiere\s*;", RegexOptions.IgnoreCase))
            {
                throw new InvalidOperationException("Le script viserait encore la base lafermiere : arrêt.");
            }

            await using var connexion = new MySqlConnection(serveur.ConnectionString);
            await connexion.OpenAsync();

            foreach (var instruction in ScriptSql.Decouper(script))
            {
                await using var commande = new MySqlCommand(instruction, connexion);
                await commande.ExecuteNonQueryAsync();
            }

            await AjouterComptesDeTestAsync(connexion);
        }

        /// <summary>
        /// Un compte par rôle, avec un mot de passe connu (haché en BCrypt comme dans l'API).
        /// Le compte admin de schema.sql n'est pas utilisable : son mot de passe est inconnu.
        /// </summary>
        private static async Task AjouterComptesDeTestAsync(MySqlConnection connexion)
        {
            foreach (var compte in ComptesDeTest.Tous)
            {
                await using var commande = new MySqlCommand(
                    "INSERT INTO Utilisateurs (Nom, Email, PasswordHash, Role) VALUES (@nom, @email, @hash, @role)",
                    connexion);
                // Paramètres (@nom...) plutôt que concaténation : protège des injections SQL
                commande.Parameters.AddWithValue("@nom", $"Test {compte.Role}");
                commande.Parameters.AddWithValue("@email", compte.Email);
                commande.Parameters.AddWithValue("@hash", PasswordHasher.HashPassword(compte.MotDePasse));
                commande.Parameters.AddWithValue("@role", compte.Role);
                await commande.ExecuteNonQueryAsync();
            }
        }

        /// <summary>Chaîne de connexion de référence (identifiants), lue hors du dépôt.</summary>
        private static string ChaineDeBase()
        {
            var depuisEnvironnement = Environment.GetEnvironmentVariable("LAFERMIERE_TEST_DB_CONNECTION");
            if (!string.IsNullOrWhiteSpace(depuisEnvironnement))
            {
                return depuisEnvironnement;
            }

            // Les user-secrets sont rattachés au projet PresentationApi (UserSecretsId du .csproj) :
            // on les lit à partir de son assembly.
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, optional: true)
                .Build();

            return configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connexion MySQL introuvable : définir LAFERMIERE_TEST_DB_CONNECTION " +
                    "ou le user-secret ConnectionStrings:DefaultConnection de PresentationApi.");
        }
    }

    /// <summary>Comptes créés dans lafermiere_tests, un par rôle de l'application.</summary>
    public record CompteDeTest(string Role, string Email, string MotDePasse);

    public static class ComptesDeTest
    {
        public static readonly CompteDeTest Admin = new("Admin", "admin.tests@ferme.local", "Admin123");
        public static readonly CompteDeTest Fermier = new("Fermier", "fermier.tests@ferme.local", "Fermier123");
        public static readonly CompteDeTest Veterinaire = new("Veterinaire", "veto.tests@ferme.local", "Veto1234");
        public static readonly CompteDeTest Comptable = new("Comptable", "compta.tests@ferme.local", "Compta123");

        public static readonly CompteDeTest[] Tous = { Admin, Fermier, Veterinaire, Comptable };

        public static CompteDeTest ParRole(string role) => Tous.Single(c => c.Role == role);
    }
}
