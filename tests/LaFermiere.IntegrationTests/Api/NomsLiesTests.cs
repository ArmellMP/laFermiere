using System.Net.Http.Json;
using System.Text.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Noms liés renvoyés par l'API (code de la bande, nom du client, du fournisseur, du créateur).
    ///
    /// Bug corrigé : les repositories lisaient ces noms par jointure SQL, mais l'entité du domaine
    /// n'avait pas de champ pour les recevoir. Ils étaient perdus et l'API renvoyait toujours null.
    /// </summary>
    [Collection("Api")]
    public class NomsLiesTests
    {
        private readonly ApiFixture _api;

        public NomsLiesTests(ApiFixture api)
        {
            _api = api;
        }

        private static async Task<JsonElement> CreerAsync(HttpClient client, string route, object corps)
        {
            var reponse = await client.PostAsJsonAsync(route, corps);
            var texte = await reponse.Content.ReadAsStringAsync();
            Assert.True(reponse.IsSuccessStatusCode, $"{route} refusé : {texte}");
            return JsonDocument.Parse(texte).RootElement;
        }

        private static async Task<string> CodeBandeAsync(HttpClient client, int bandeId) =>
            (await Json(await client.GetAsync($"/api/v1/Bandes/{bandeId}"))).GetProperty("code").GetString()!;

        [Fact]
        public async Task Vente_RenvoieCodeBandeNomClientEtCreateur()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var client = await CreerClientAsync(admin);
            var id = (await CreerAsync(admin, "/api/v1/Ventes", new
            {
                bandeId = bande, clientId = client, dateVente = DateTime.Today,
                typeVente = "PouletsVivants", quantite = 2, uniteMesure = "Unite", prixUnitaire = 2500,
                modePaiement = "Especes", estPaye = false
            })).GetProperty("id").GetInt32();

            var vente = await Json(await admin.GetAsync($"/api/v1/Ventes/{id}"));

            Assert.Equal(await CodeBandeAsync(admin, bande), vente.GetProperty("bandeCode").GetString());
            Assert.Equal("Client test", vente.GetProperty("clientNom").GetString());
            Assert.False(string.IsNullOrEmpty(vente.GetProperty("createurNom").GetString()));
        }

        [Fact]
        public async Task Achat_RenvoieCodeBandeEtNomFournisseur()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);
            var id = (await CreerAsync(admin, "/api/v1/Achats", new
            {
                fournisseurId = fournisseur, bandeId = bande, dateAchat = DateTime.Today.AddDays(-1),
                typeAchat = "Aliment", quantite = 20, uniteMesure = "Kg", prixUnitaire = 300
            })).GetProperty("id").GetInt32();

            var achat = await Json(await admin.GetAsync($"/api/v1/Achats/{id}"));

            Assert.Equal(await CodeBandeAsync(admin, bande), achat.GetProperty("bandeCode").GetString());
            Assert.Equal("Fournisseur test", achat.GetProperty("fournisseurNom").GetString());
        }

        [Fact]
        public async Task Stock_RenvoieCodeBande_DansLaListeEtLeDetail()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var id = (await CreerAsync(admin, "/api/v1/Stocks", new
            {
                bandeId = bande, type = "Aliment", nom = "Croissance", quantiteInitiale = 40, uniteMesure = "Kg"
            })).GetProperty("id").GetInt32();
            var code = await CodeBandeAsync(admin, bande);

            var detail = await Json(await admin.GetAsync($"/api/v1/Stocks/{id}"));
            var duneBande = await Json(await admin.GetAsync($"/api/v1/Stocks/bande/{bande}"));

            Assert.Equal(code, detail.GetProperty("bandeCode").GetString());
            Assert.Equal(code, duneBande.EnumerateArray().Single().GetProperty("bandeCode").GetString());
        }

        [Fact]
        public async Task Suivi_RenvoieCodeBandeEtCreateur()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            await CreerAsync(admin, "/api/v1/SuiviJournalier", new { bandeId = bande, jour = 1, dateSuivi = DateArrivee, nbMortalites = 0 });

            var suivi = (await Json(await admin.GetAsync($"/api/v1/SuiviJournalier/bande/{bande}"))).EnumerateArray().Single();

            Assert.Equal(await CodeBandeAsync(admin, bande), suivi.GetProperty("bandeCode").GetString());
            Assert.False(string.IsNullOrEmpty(suivi.GetProperty("createurNom").GetString()));
        }
    }
}
