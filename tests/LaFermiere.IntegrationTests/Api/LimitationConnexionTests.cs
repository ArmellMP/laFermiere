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
    /// Limitation des tentatives de connexion : au-delà de TentativesMax essais par fenêtre,
    /// /authenticate répond 429 Too Many Requests (format ErrorResponse + en-tête Retry-After).
    ///
    /// Chaque test démarre sa propre API (limite à 3 essais) : son compteur est neuf et
    /// n'est pas partagé avec les autres tests. L'API partagée d'ApiFixture n'a pas de limite.
    /// </summary>
    [Collection("Api")]
    public class LimitationConnexionTests
    {
        private const int Limite = 3;
        private const string RouteConnexion = "/api/v1/Utilisateurs/authenticate";

        private readonly ApiFixture _api;

        public LimitationConnexionTests(ApiFixture api)
        {
            _api = api;
        }

        private static WebApplicationFactory<Program> ApiLimitee() =>
            new WebApplicationFactory<Program>().WithWebHostBuilder(hote =>
                hote.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LimitationConnexion:TentativesMax"] = Limite.ToString(),
                    ["LimitationConnexion:FenetreSecondes"] = "60"
                })));

        private static Task<HttpResponseMessage> ConnexionAsync(HttpClient client, string motDePasse) =>
            client.PostAsJsonAsync(RouteConnexion, new { email = ComptesDeTest.Admin.Email, password = motDePasse });

        [Fact]
        public async Task AuDelaDeLaLimite_429AvecRetryAfterEtFormatCommun()
        {
            await using var api = ApiLimitee();
            var client = api.CreateClient();

            for (var i = 0; i < Limite; i++)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, (await ConnexionAsync(client, "MauvaisMotDePasse1")).StatusCode);
            }

            var reponse = await ConnexionAsync(client, "MauvaisMotDePasse1");

            Assert.Equal(HttpStatusCode.TooManyRequests, reponse.StatusCode);
            Assert.True(reponse.Headers.RetryAfter?.Delta > TimeSpan.Zero);
            var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(corps.GetProperty("success").GetBoolean());
            Assert.StartsWith("Trop de tentatives de connexion", corps.GetProperty("message").GetString());
            Assert.False(string.IsNullOrEmpty(corps.GetProperty("traceId").GetString()));
        }

        [Fact]
        public async Task LimiteAtteinte_MemeLeBonMotDePasseEstRefuse()
        {
            await using var api = ApiLimitee();
            var client = api.CreateClient();

            for (var i = 0; i < Limite; i++)
            {
                await ConnexionAsync(client, "MauvaisMotDePasse1");
            }

            // Sinon un attaquant saurait qu'il a trouvé le mot de passe (réponse 200 au lieu de 429)
            var reponse = await ConnexionAsync(client, ComptesDeTest.Admin.MotDePasse);

            Assert.Equal(HttpStatusCode.TooManyRequests, reponse.StatusCode);
        }

        [Fact]
        public async Task LimiteAtteinte_LesAutresRoutesRestentAccessibles()
        {
            await using var api = ApiLimitee();
            var client = api.CreateClient();

            for (var i = 0; i <= Limite; i++)
            {
                await ConnexionAsync(client, "MauvaisMotDePasse1");
            }

            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        }

        [Fact]
        public async Task TentativesMaxZero_PasDeLimite()
        {
            // L'API partagée est configurée avec TentativesMax = 0 (voir ApiFixture)
            var client = _api.ClientAnonyme();

            for (var i = 0; i < 15; i++)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, (await ConnexionAsync(client, "MauvaisMotDePasse1")).StatusCode);
            }
        }
    }
}
