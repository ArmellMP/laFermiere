using LaFermiere.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// CORS : un navigateur n'autorise une page web à appeler l'API que si la réponse
    /// contient l'en-tête Access-Control-Allow-Origin pour l'origine de cette page.
    ///   Développement : toute origine est acceptée.
    ///   Production    : seulement les origines de Cors:OriginesAutorisees (aucune par défaut).
    ///
    /// Collection "Api" : ApiFixture a déjà préparé la base et les variables d'environnement
    /// (connexion, clé JWT) que les API démarrées ici réutilisent.
    /// </summary>
    [Collection("Api")]
    public class CorsTests
    {
        private const string OrigineFront = "https://front.lafermiere.test";
        private const string OrigineInconnue = "https://site-inconnu.test";

        public CorsTests(ApiFixture api)
        {
            _ = api;   // garantit l'initialisation de la fixture avant ces tests
        }

        /// <summary>
        /// Requête "preflight" envoyée par le navigateur avant un vrai appel :
        /// renvoie la valeur de Access-Control-Allow-Origin, ou null si l'origine est refusée.
        /// </summary>
        private static async Task<string?> OrigineAccepteeAsync(WebApplicationFactory<Program> api, string origine)
        {
            using var requete = new HttpRequestMessage(HttpMethod.Options, "/api/v1/Bandes");
            requete.Headers.Add("Origin", origine);
            requete.Headers.Add("Access-Control-Request-Method", "GET");
            requete.Headers.Add("Access-Control-Request-Headers", "authorization");

            var reponse = await api.CreateClient().SendAsync(requete);
            return reponse.Headers.TryGetValues("Access-Control-Allow-Origin", out var valeurs)
                ? valeurs.Single()
                : null;
        }

        private static WebApplicationFactory<Program> ApiEnProduction(params string[] origines) =>
            new WebApplicationFactory<Program>().WithWebHostBuilder(hote =>
            {
                hote.UseEnvironment("Production");
                for (var i = 0; i < origines.Length; i++)
                {
                    hote.UseSetting($"Cors:OriginesAutorisees:{i}", origines[i]);
                }
            });

        [Fact]
        public async Task Developpement_ToutesOriginesAcceptees()
        {
            await using var api = new WebApplicationFactory<Program>().WithWebHostBuilder(h => h.UseEnvironment("Development"));

            Assert.Equal("*", await OrigineAccepteeAsync(api, OrigineInconnue));
        }

        [Fact]
        public async Task Production_SeuleLOrigineConfigureeEstAcceptee()
        {
            await using var api = ApiEnProduction(OrigineFront);

            Assert.Equal(OrigineFront, await OrigineAccepteeAsync(api, OrigineFront));
            Assert.Null(await OrigineAccepteeAsync(api, OrigineInconnue));
        }

        [Fact]
        public async Task Production_SansConfiguration_AucuneOrigineAcceptee()
        {
            await using var api = ApiEnProduction();

            Assert.Null(await OrigineAccepteeAsync(api, OrigineFront));
        }
    }
}
