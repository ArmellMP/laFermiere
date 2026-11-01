using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Format unique des erreurs (ErrorResponse, DECISIONS.md 2026-11-01) :
    ///   { "success": false, "message": "...", "errors": { "champ": ["..."] }, "traceId": "..." }
    /// Vérifié pour chaque origine d'erreur possible.
    /// </summary>
    [Collection("Api")]
    public class FormatErreurTests
    {
        private readonly ApiFixture _api;

        public FormatErreurTests(ApiFixture api)
        {
            _api = api;
        }

        // Vérifications communes à toutes les erreurs
        private static void VerifierFormatCommun(JsonElement corps)
        {
            Assert.False(corps.GetProperty("success").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(corps.GetProperty("message").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(corps.GetProperty("traceId").GetString()));
        }

        [Fact]
        public async Task ErreurDataAnnotations_FormatCommunAvecChampEnCamelCase()
        {
            var admin = await _api.ClientAsync("Admin");

            // nbPoussinsInit absent : refusé par [Range] sur la Request (couche HTTP)
            var reponse = await admin.PostAsJsonAsync("/api/v1/Bandes", new { dateArrivee = DateArrivee });
            var corps = await Json(reponse);

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            VerifierFormatCommun(corps);
            Assert.True(corps.GetProperty("errors").TryGetProperty("nbPoussinsInit", out _));
        }

        [Fact]
        public async Task ErreurFluentValidation_MemeFormat()
        {
            var admin = await _api.ClientAsync("Admin");

            // Date future : acceptée par les DataAnnotations, refusée par FluentValidation (décision n°2)
            var reponse = await admin.PostAsJsonAsync("/api/v1/Bandes",
                new { dateArrivee = DateTime.Today.AddDays(5), nbPoussinsInit = 100 });
            var corps = await Json(reponse);

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            VerifierFormatCommun(corps);
            Assert.True(corps.GetProperty("errors").TryGetProperty("dateArrivee", out _));
        }

        [Fact]
        public async Task RessourceIntrouvable_404AuFormatCommun()
        {
            var admin = await _api.ClientAsync("Admin");

            var reponse = await admin.GetAsync("/api/v1/Bandes/999999");
            var corps = await Json(reponse);

            Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
            VerifierFormatCommun(corps);
        }

        [Fact]
        public async Task IdUrlDifferentDuCorps_400AuFormatCommun()
        {
            var admin = await _api.ClientAsync("Admin");
            var bandeId = await CreerBandeAsync(admin);

            // Ancien BadRequest("texte") du contrôleur, désormais au format commun
            var reponse = await admin.PutAsJsonAsync($"/api/v1/Bandes/{bandeId}",
                new { id = bandeId + 1, nbPoussinsActuel = 10, statut = "EnCours" });
            var corps = await Json(reponse);

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            VerifierFormatCommun(corps);
        }
    }
}
