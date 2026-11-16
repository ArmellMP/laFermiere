using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Health checks :
    ///   /health       → 200 si l'API et la base répondent, 503 si la base est injoignable
    ///   /health/live  → 200 tant que le processus tourne, même sans base
    ///
    /// Collection "Api" : ApiFixture a déjà préparé la base et les variables d'environnement
    /// (connexion, clé JWT) que les API démarrées ici réutilisent.
    /// </summary>
    [Collection("Api")]
    public class HealthChecksTests
    {
        // Port 1 : rien n'y écoute, la connexion est refusée immédiatement
        private const string BaseInjoignable =
            "Server=127.0.0.1;Port=1;Database=inexistante;User=root;ConnectionTimeout=2";

        private readonly ApiFixture _api;

        public HealthChecksTests(ApiFixture api)
        {
            _api = api;
        }

        /// <summary>API dont la chaîne de connexion pointe vers un serveur MySQL absent.</summary>
        private static WebApplicationFactory<Program> ApiSansBase(string environnement) =>
            new WebApplicationFactory<Program>().WithWebHostBuilder(hote =>
            {
                hote.UseEnvironment(environnement);
                // Source ajoutée en dernier : prioritaire sur la variable d'environnement de la fixture
                hote.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = BaseInjoignable
                }));
            });

        private static JsonElement VerificationMySql(JsonElement corps) =>
            corps.GetProperty("verifications").EnumerateArray().Single(v => v.GetProperty("nom").GetString() == "mysql");

        [Fact]
        public async Task Health_BaseDisponible_200EtMySqlHealthy()
        {
            var reponse = await _api.ClientAnonyme().GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Healthy", corps.GetProperty("statut").GetString());
            Assert.Equal("Healthy", VerificationMySql(corps).GetProperty("statut").GetString());
        }

        [Fact]
        public async Task HealthLive_NeVerifiePasLaBase()
        {
            var reponse = await _api.ClientAnonyme().GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Healthy", corps.GetProperty("statut").GetString());
            Assert.Equal(0, corps.GetProperty("verifications").GetArrayLength());
        }

        [Fact]
        public async Task Health_BaseInjoignable_503EtDetailEnDeveloppement()
        {
            await using var api = ApiSansBase("Development");
            var client = api.CreateClient();

            var reponse = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, reponse.StatusCode);
            var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Unhealthy", corps.GetProperty("statut").GetString());
            var mysql = VerificationMySql(corps);
            Assert.Equal("Unhealthy", mysql.GetProperty("statut").GetString());
            Assert.False(string.IsNullOrWhiteSpace(mysql.GetProperty("erreur").GetString()));

            // Le processus, lui, tourne toujours
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        }

        [Fact]
        public async Task Health_BaseInjoignable_PasDeDetailEnProduction()
        {
            await using var api = ApiSansBase("Production");

            var reponse = await api.CreateClient().GetAsync("/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, reponse.StatusCode);
            var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();
            // Pas de message MySQL (nom du serveur, port...) exposé publiquement
            Assert.False(VerificationMySql(corps).TryGetProperty("erreur", out _));
        }
    }
}
