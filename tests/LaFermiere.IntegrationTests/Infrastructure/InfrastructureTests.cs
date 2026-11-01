using System.Net;
using MySqlConnector;

namespace LaFermiere.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Tests "de fumée" de l'infrastructure : si ceux-là échouent, inutile de lire les autres.
    /// Ils vérifient que la base de test est complète et que l'API démarre dessus.
    ///
    /// [Collection("Api")] : ce test reçoit l'ApiFixture partagée (voir ApiFixture.cs).
    /// xUnit la passe automatiquement au constructeur.
    /// </summary>
    [Collection("Api")]
    public class InfrastructureTests
    {
        private readonly ApiFixture _api;

        public InfrastructureTests(ApiFixture api)
        {
            _api = api;
        }

        [Fact]
        public async Task SchemaSql_CreeToutesLesTablesTriggersEtRoutines()
        {
            // On interroge le catalogue de MySQL (information_schema) sur la base de test
            await using var connexion = new MySqlConnection(BaseDeTest.ChaineDeConnexion());
            await connexion.OpenAsync();

            async Task<long> Compter(string sql)
            {
                await using var commande = new MySqlCommand(sql, connexion);
                commande.Parameters.AddWithValue("@base", BaseDeTest.NomBase);
                return Convert.ToInt64(await commande.ExecuteScalarAsync());
            }

            Assert.Equal(10, await Compter(
                "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @base AND TABLE_TYPE = 'BASE TABLE'"));
            // 3 triggers d'achat (migration 003) + 3 triggers de suivi journalier
            Assert.Equal(6, await Compter(
                "SELECT COUNT(*) FROM information_schema.TRIGGERS WHERE TRIGGER_SCHEMA = @base"));
            // fn_type_stock_achat + sp_ajuster_stock_achat
            Assert.Equal(2, await Compter(
                "SELECT COUNT(*) FROM information_schema.ROUTINES WHERE ROUTINE_SCHEMA = @base"));
        }

        [Fact]
        public async Task Api_Demarre_HealthRepond200()
        {
            var reponse = await _api.ClientAnonyme().GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        }

        // Chaque compte de test obtient un jeton : ClientAsync échouerait sinon
        [Theory]
        [InlineData("Admin")]
        [InlineData("Fermier")]
        [InlineData("Veterinaire")]
        [InlineData("Comptable")]
        public async Task ComptesDeTest_PeuventSeConnecter(string role)
        {
            var client = await _api.ClientAsync(role);

            Assert.NotNull(client.DefaultRequestHeaders.Authorization);
        }
    }
}
