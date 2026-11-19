using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Doublons : une donnée qui existe déjà donne 409 Conflict (format ErrorResponse),
    /// et non plus 400 (vérification du handler) ou 500 (contrainte UNIQUE de la base).
    /// </summary>
    [Collection("Api")]
    public class ConflitsTests
    {
        private readonly ApiFixture _api;

        public ConflitsTests(ApiFixture api)
        {
            _api = api;
        }

        private static async Task<string> Verifier409Async(HttpResponseMessage reponse)
        {
            var corps = await Json(reponse);
            Assert.Equal(HttpStatusCode.Conflict, reponse.StatusCode);
            Assert.False(corps.GetProperty("success").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(corps.GetProperty("traceId").GetString()));
            return corps.GetProperty("message").GetString()!;
        }

        // Identifiant unique composé de lettres et chiffres (clés de paramètre, e-mails)
        private static string Unique() => Guid.NewGuid().ToString("N")[..12];

        [Fact]
        public async Task Stock_MemeTypePourLaMemeBande_409ParLaContrainteDeLaBase()
        {
            // Aucun contrôle dans le handler : avant, la clé UNIQUE (BandeId, Type) donnait une 500
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            object Stock() => new { bandeId = bande, type = "Aliment", nom = "Démarrage", quantiteInitiale = 50, uniteMesure = "Kg" };

            Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/v1/Stocks", Stock())).StatusCode);

            var message = await Verifier409Async(await admin.PostAsJsonAsync("/api/v1/Stocks", Stock()));
            Assert.StartsWith("Un stock de ce type existe déjà pour cette bande", message);
        }

        [Fact]
        public async Task Utilisateur_EmailDejaUtilise_409()
        {
            var admin = await _api.ClientAsync("Admin");
            object Utilisateur() => new { nom = "Doublon", email = $"doublon{_suffixe}@test.local", password = "MotDePasse1", role = "Fermier" };

            Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/v1/Utilisateurs", Utilisateur())).StatusCode);

            var message = await Verifier409Async(await admin.PostAsJsonAsync("/api/v1/Utilisateurs", Utilisateur()));
            Assert.Contains("existe déjà", message);
        }

        private readonly string _suffixe = Unique();

        [Fact]
        public async Task Parametre_CleDejaPrise_409()
        {
            var admin = await _api.ClientAsync("Admin");
            var cle = "Cle_" + Unique();
            object Parametre() => new { cle, valeur = "1", categorie = "General" };

            Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/v1/Parametres", Parametre())).StatusCode);

            await Verifier409Async(await admin.PostAsJsonAsync("/api/v1/Parametres", Parametre()));
        }

        [Fact]
        public async Task Suivi_DejaSaisiPourCetteDate_409()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            object Suivi() => new { bandeId = bande, jour = 1, dateSuivi = DateArrivee, nbMortalites = 0 };

            Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/v1/SuiviJournalier", Suivi())).StatusCode);

            var message = await Verifier409Async(await admin.PostAsJsonAsync("/api/v1/SuiviJournalier", Suivi()));
            Assert.StartsWith("Un suivi existe déjà", message);
        }
    }
}
