using System.Net;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Les listes racines (GET /api/v1/Xxx) renvoient une page :
    /// { items, page, taille, total, totalPages }, réglée par ?page= et ?taille=.
    /// </summary>
    [Collection("Api")]
    public class PaginationTests
    {
        private readonly ApiFixture _api;

        public PaginationTests(ApiFixture api)
        {
            _api = api;
        }

        [Fact]
        public async Task ListeFiltree_DecoupeeEnPages()
        {
            var admin = await _api.ClientAsync("Admin");
            // Nom propre à ce test : le filtre searchTerm isole ces 3 clients.
            // Lettres uniquement (règle du nom d'un client) : chiffres du Guid convertis en lettres.
            var nom = "Pagination" + new string(Guid.NewGuid().ToString("N").Select(c => (char)('a' + Convert.ToInt32(c.ToString(), 16))).ToArray());
            foreach (var suffixe in new[] { "A", "B", "C" })
            {
                var creation = await admin.PostAsJsonAsync("/api/v1/Clients",
                    new { nom = $"{nom} {suffixe}", typeClient = "Particulier", telephone = "690000003" });
                Assert.Equal(HttpStatusCode.Created, creation.StatusCode);
            }

            var page1 = await Json(await admin.GetAsync($"/api/v1/Clients?searchTerm={nom}&taille=2"));
            var page2 = await Json(await admin.GetAsync($"/api/v1/Clients?searchTerm={nom}&taille=2&page=2"));

            Assert.Equal(2, page1.GetProperty("items").GetArrayLength());
            Assert.Equal(3, page1.GetProperty("total").GetInt32());
            Assert.Equal(2, page1.GetProperty("totalPages").GetInt32());
            Assert.Equal(1, page2.GetProperty("items").GetArrayLength());
            Assert.Equal(2, page2.GetProperty("page").GetInt32());
        }

        [Fact]
        public async Task SansParametre_PremierePageDeVingt()
        {
            var admin = await _api.ClientAsync("Admin");

            var page = await Json(await admin.GetAsync("/api/v1/Bandes"));

            Assert.Equal(1, page.GetProperty("page").GetInt32());
            Assert.Equal(20, page.GetProperty("taille").GetInt32());
            Assert.True(page.GetProperty("items").GetArrayLength() <= 20);
        }

        [Theory]
        [InlineData("taille=0", "taille")]
        [InlineData("taille=101", "taille")]
        [InlineData("page=0", "page")]
        public async Task ParametreHorsLimites_400AuFormatErrorResponse(string parametre, string champ)
        {
            var admin = await _api.ClientAsync("Admin");

            var reponse = await admin.GetAsync($"/api/v1/Ventes?{parametre}");

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            var erreurs = (await Json(reponse)).GetProperty("errors");
            Assert.True(erreurs.TryGetProperty(champ, out _), $"Erreur attendue sur '{champ}' : {erreurs}");
        }
    }
}
