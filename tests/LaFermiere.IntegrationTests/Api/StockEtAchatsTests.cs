using System.Net;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Le stock suit les achats grâce aux TRIGGERS SQL (migration 003) :
    /// ces règles vivent dans la base, elles ne peuvent être testées que sur une vraie base.
    ///   création     : + quantité
    ///   modification : + ou - la différence ; changement de type = sortie puis entrée
    ///   suppression  : - quantité
    ///   règle        : refus (400) si la quantité à retirer est déjà consommée
    /// </summary>
    [Collection("Api")]
    public class StockEtAchatsTests
    {
        private readonly ApiFixture _api;

        public StockEtAchatsTests(ApiFixture api)
        {
            _api = api;
        }

        private static async Task<int> AcheterAsync(HttpClient admin, int bandeId, int fournisseurId,
            string type, decimal quantite, string unite)
        {
            var reponse = await admin.PostAsJsonAsync("/api/v1/Achats", new
            {
                fournisseurId,
                bandeId,
                dateAchat = DateTime.Today.AddDays(-3),
                typeAchat = type,
                quantite,
                uniteMesure = unite,
                prixUnitaire = 300
            });
            var corps = await reponse.Content.ReadAsStringAsync();
            Assert.True(reponse.IsSuccessStatusCode, $"Achat refusé : {corps}");
            return System.Text.Json.JsonDocument.Parse(corps).RootElement.GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task CycleDeVieDUnAchat_LeStockSuit()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);

            // Création : le trigger crée le stock
            var achat = await AcheterAsync(admin, bande, fournisseur, "Aliment", 50, "Kg");
            Assert.Equal((50m, 50m), await StockAsync(admin, bande, "Aliment"));

            // Modification 50 -> 60 : +10
            var modif = await admin.PutAsJsonAsync($"/api/v1/Achats/{achat}", new { id = achat, quantite = 60 });
            Assert.Equal(HttpStatusCode.OK, modif.StatusCode);
            Assert.Equal((60m, 60m), await StockAsync(admin, bande, "Aliment"));

            // Suppression : -60, le stock revient à 0
            var suppression = await admin.DeleteAsync($"/api/v1/Achats/{achat}");
            Assert.Equal(HttpStatusCode.NoContent, suppression.StatusCode);
            Assert.Equal((0m, 0m), await StockAsync(admin, bande, "Aliment"));
        }

        [Fact]
        public async Task QuantiteDejaConsommee_ModificationEtSuppressionRefusees()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);
            var achat = await AcheterAsync(admin, bande, fournisseur, "Aliment", 50, "Kg");

            // 45 Kg consommés : il en reste 5
            var stock = (await Json(await admin.GetAsync($"/api/v1/Stocks/bande/{bande}")))[0].GetProperty("id").GetInt32();
            var conso = await admin.PostAsJsonAsync($"/api/v1/Stocks/{stock}/consommer",
                new { stockId = stock, quantiteConsommee = 45, suiviJournalierId = (int?)null });
            Assert.Equal(HttpStatusCode.OK, conso.StatusCode);

            // Ramener l'achat à 20 Kg retirerait 30 Kg : refusé, message du trigger
            var modif = await admin.PutAsJsonAsync($"/api/v1/Achats/{achat}", new { id = achat, quantite = 20 });
            Assert.Equal(HttpStatusCode.BadRequest, modif.StatusCode);
            Assert.Contains("insuffisant", (await Json(modif)).GetProperty("message").GetString());

            // Supprimer l'achat retirerait 50 Kg : refusé aussi
            var suppression = await admin.DeleteAsync($"/api/v1/Achats/{achat}");
            Assert.Equal(HttpStatusCode.BadRequest, suppression.StatusCode);

            // Rien n'a bougé : ni le stock, ni l'achat
            Assert.Equal((50m, 5m), await StockAsync(admin, bande, "Aliment"));
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/v1/Achats/{achat}")).StatusCode);
        }

        [Fact]
        public async Task ChangementDeType_SortDUnStockEtEntreDansLAutre()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);
            var achat = await AcheterAsync(admin, bande, fournisseur, "Medicament", 10, "Dose");

            var modif = await admin.PutAsJsonAsync($"/api/v1/Achats/{achat}", new { id = achat, typeAchat = "Materiel" });

            Assert.Equal(HttpStatusCode.OK, modif.StatusCode);
            Assert.Equal((0m, 0m), await StockAsync(admin, bande, "Medicament"));
            Assert.Equal((10m, 10m), await StockAsync(admin, bande, "Materiel"));
        }

        [Fact]
        public async Task AchatHorsStock_NeCreeAucunStock()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);

            await AcheterAsync(admin, bande, fournisseur, "Transport", 120, "Km");

            var stocks = await Json(await admin.GetAsync($"/api/v1/Stocks/bande/{bande}"));
            Assert.Equal(0, stocks.GetArrayLength());
        }

        [Fact]
        public async Task AchatDePoussins_AjouteALaBandeEtPasAuStock()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin, nbPoussins: 100);
            var fournisseur = await CreerFournisseurAsync(admin);

            await AcheterAsync(admin, bande, fournisseur, "Poussins", 20, "Unite");

            Assert.Equal((120, 120), await EffectifAsync(admin, bande));
            Assert.Null(await StockAsync(admin, bande, "Autre"));
        }
    }
}
