using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Gestion des comptes par l'administrateur : un compte qui a déjà saisi des données
    /// ne peut pas être supprimé (409 avec un conseil, au lieu d'une 500), mais il peut être désactivé.
    /// </summary>
    [Collection("Api")]
    public class UtilisateursGestionTests
    {
        private readonly ApiFixture _api;

        public UtilisateursGestionTests(ApiFixture api)
        {
            _api = api;
        }

        private async Task<(int Id, string Email)> CreerFermierAsync(HttpClient admin)
        {
            var email = $"fermier{Guid.NewGuid():N}"[..20] + "@test.local";
            var reponse = await admin.PostAsJsonAsync("/api/v1/Utilisateurs",
                new { nom = "Fermier de test", email, password = "Fermier123", role = "Fermier" });
            Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
            return ((await Json(reponse)).GetProperty("id").GetInt32(), email);
        }

        private async Task<HttpClient> ConnecterAsync(string email)
        {
            var reponse = await _api.ClientAnonyme().PostAsJsonAsync("/api/v1/Utilisateurs/authenticate", new { email, password = "Fermier123" });
            reponse.EnsureSuccessStatusCode();
            var client = _api.ClientAnonyme();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await Json(reponse)).GetProperty("token").GetString());
            return client;
        }

        /// <summary>Une alerte « stock faible » : 50 kg, seuil 10 kg, 45 kg consommés.</summary>
        private static async Task<int> CreerAlerteAsync(HttpClient admin)
        {
            var bande = await CreerBandeAsync(admin);
            var stock = await admin.PostAsJsonAsync("/api/v1/Stocks", new
            {
                bandeId = bande, type = "Aliment", nom = "Aliment compte", quantiteInitiale = 50, uniteMesure = "Kg", seuilAlerte = 10
            });
            var stockId = (await Json(stock)).GetProperty("id").GetInt32();
            (await admin.PostAsJsonAsync($"/api/v1/Stocks/{stockId}/consommer", new { stockId, quantiteConsommee = 45 })).EnsureSuccessStatusCode();
            (await admin.PostAsync("/api/v1/Alertes/generer", null)).EnsureSuccessStatusCode();

            var alertes = await Json(await admin.GetAsync("/api/v1/Alertes/non-lues"));
            return alertes.EnumerateArray()
                .First(a => a.GetProperty("typeAlerte").GetString() == "StockFaible" && a.GetProperty("entiteId").GetInt32() == stockId)
                .GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task Supprimer_UnCompteSansDonnees_204()
        {
            var admin = await _api.ClientAsync("Admin");
            var (id, _) = await CreerFermierAsync(admin);

            Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/v1/Utilisateurs/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/v1/Utilisateurs/{id}")).StatusCode);
        }

        [Fact]
        public async Task Supprimer_UnCompteQuiASaisiDesDonnees_409_PuisDesactivationPossible()
        {
            var admin = await _api.ClientAsync("Admin");
            var (id, email) = await CreerFermierAsync(admin);

            // Le fermier lit une alerte : elle garde son identifiant (Alertes.UtilisateurId, clé étrangère)
            var fermier = await ConnecterAsync(email);
            var alerteId = await CreerAlerteAsync(admin);
            var marquage = await fermier.PostAsJsonAsync("/api/v1/Alertes/marquer",
                new { alerteIds = new[] { alerteId }, utilisateurId = id, marquerToutes = false });
            Assert.True(marquage.IsSuccessStatusCode, await marquage.Content.ReadAsStringAsync());

            // Avant : erreur de clé étrangère MySQL -> 500
            var suppression = await admin.DeleteAsync($"/api/v1/Utilisateurs/{id}");
            var corps = await Json(suppression);
            Assert.Equal(HttpStatusCode.Conflict, suppression.StatusCode);
            Assert.Contains("Désactivez-le", corps.GetProperty("message").GetString());

            // La solution proposée fonctionne : le compte est désactivé, ses données restent
            var desactivation = await admin.PutAsJsonAsync($"/api/v1/Utilisateurs/{id}", new { id, estActif = false });
            Assert.Equal(HttpStatusCode.OK, desactivation.StatusCode);
            Assert.False((await Json(desactivation)).GetProperty("estActif").GetBoolean());

            // Et il ne peut plus se connecter
            var connexion = await _api.ClientAnonyme().PostAsJsonAsync("/api/v1/Utilisateurs/authenticate", new { email, password = "Fermier123" });
            Assert.False(connexion.IsSuccessStatusCode);
        }
    }
}
