using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Génération des alertes métier (POST /Alertes/generer) et statistiques
    /// des fiches client / fournisseur, sur une vraie base.
    /// La tâche de fond est désactivée pendant les tests (voir ApiFixture).
    /// </summary>
    [Collection("Api")]
    public class AlertesEtStatistiquesTests
    {
        private readonly ApiFixture _api;

        public AlertesEtStatistiquesTests(ApiFixture api)
        {
            _api = api;
        }

        // ============================================
        // GÉNÉRATION DES ALERTES
        // ============================================

        [Fact]
        public async Task StockFaible_AlerteCreeeUneSeuleFois()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            // 50 kg avec un seuil de 10 kg, dont 45 consommés : il reste 5 kg, stock faible
            var stock = await admin.PostAsJsonAsync("/api/v1/Stocks", new
            {
                bandeId = bande, type = "Aliment", nom = "Aliment alerte", quantiteInitiale = 50,
                uniteMesure = "Kg", seuilAlerte = 10
            });
            Assert.Equal(HttpStatusCode.Created, stock.StatusCode);
            var stockId = (await Json(stock)).GetProperty("id").GetInt32();

            var consommation = await admin.PostAsJsonAsync($"/api/v1/Stocks/{stockId}/consommer",
                new { stockId, quantiteConsommee = 45 });
            Assert.True(consommation.IsSuccessStatusCode, await consommation.Content.ReadAsStringAsync());

            var premiere = await admin.PostAsync("/api/v1/Alertes/generer", null);
            Assert.Equal(HttpStatusCode.OK, premiere.StatusCode);
            Assert.True((await Json(premiere)).GetProperty("stockFaible").GetInt32() >= 1);

            // Deuxième passage : l'alerte non lue existe déjà, pas de doublon pour ce stock
            Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync("/api/v1/Alertes/generer", null)).StatusCode);

            var alertes = await AlertesNonLuesAsync(admin, "StockFaible", stockId);
            var alerte = Assert.Single(alertes);
            Assert.Contains("Aliment alerte", alerte.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GenererLesAlertes_ReserveAuxAdminEtFermier()
        {
            var comptable = await _api.ClientAsync("Comptable");
            var fermier = await _api.ClientAsync("Fermier");

            Assert.Equal(HttpStatusCode.Forbidden, (await comptable.PostAsync("/api/v1/Alertes/generer", null)).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await fermier.PostAsync("/api/v1/Alertes/generer", null)).StatusCode);
        }

        private static async Task<List<JsonElement>> AlertesNonLuesAsync(HttpClient client, string type, int entiteId)
        {
            var toutes = await Json(await client.GetAsync("/api/v1/Alertes/non-lues"));
            return toutes.EnumerateArray()
                .Where(a => a.GetProperty("typeAlerte").GetString() == type && a.GetProperty("entiteId").GetInt32() == entiteId)
                .ToList();
        }

        // ============================================
        // STATISTIQUES CLIENT / FOURNISSEUR
        // ============================================

        [Fact]
        public async Task FicheClient_NombreEtMontantDesVentes()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin, nbPoussins: 100);
            var client = await CreerClientAsync(admin);

            foreach (var quantite in new[] { 4, 6 })
            {
                var vente = await admin.PostAsJsonAsync("/api/v1/Ventes", new
                {
                    bandeId = bande, clientId = client, dateVente = DateTime.Today.AddDays(-1),
                    typeVente = "PouletsVivants", quantite, uniteMesure = "Unite", prixUnitaire = 2500,
                    modePaiement = "Especes", estPaye = false
                });
                Assert.Equal(HttpStatusCode.Created, vente.StatusCode);
            }

            var fiche = await Json(await admin.GetAsync($"/api/v1/Clients/{client}"));

            Assert.Equal(2, fiche.GetProperty("nombreVentes").GetInt32());
            Assert.Equal(25000m, fiche.GetProperty("montantTotalAchats").GetDecimal());   // (4 + 6) x 2500
        }

        [Fact]
        public async Task FicheFournisseur_NombreEtMontantDesAchats_ZeroSansAchat()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);

            var vide = await Json(await admin.GetAsync($"/api/v1/Fournisseurs/{fournisseur}"));
            Assert.Equal(0, vide.GetProperty("nombreAchats").GetInt32());
            Assert.Equal(0m, vide.GetProperty("montantTotalAchats").GetDecimal());

            foreach (var quantite in new[] { 10, 20 })
            {
                var achat = await admin.PostAsJsonAsync("/api/v1/Achats", new
                {
                    fournisseurId = fournisseur, bandeId = bande, dateAchat = DateTime.Today.AddDays(-3),
                    typeAchat = "Aliment", quantite, uniteMesure = "Kg", prixUnitaire = 300
                });
                Assert.True(achat.IsSuccessStatusCode, await achat.Content.ReadAsStringAsync());
            }

            var fiche = await Json(await admin.GetAsync($"/api/v1/Fournisseurs/{fournisseur}"));

            Assert.Equal(2, fiche.GetProperty("nombreAchats").GetInt32());
            Assert.Equal(9000m, fiche.GetProperty("montantTotalAchats").GetDecimal());   // (10 + 20) x 300
        }
    }
}
