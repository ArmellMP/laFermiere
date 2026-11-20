using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// L'auteur d'une saisie est l'utilisateur connecté (son jeton). Avant : tous les suivis,
    /// ventes et achats étaient enregistrés au nom de l'utilisateur n° 1, et le marquage d'une
    /// alerte faisait confiance à l'utilisateurId envoyé dans le corps de la requête.
    /// </summary>
    [Collection("Api")]
    public class AuteurDesSaisiesTests
    {
        private readonly ApiFixture _api;

        public AuteurDesSaisiesTests(ApiFixture api)
        {
            _api = api;
        }

        /// <summary>Un compte neuf avec un nom unique, déjà connecté.</summary>
        private async Task<(HttpClient Client, int Id, string Nom)> NouveauCompteAsync(string role)
        {
            var admin = await _api.ClientAsync("Admin");
            var suffixe = Guid.NewGuid().ToString("N")[..10];
            var nom = $"{role} {suffixe}";
            var email = $"{role.ToLowerInvariant()}{suffixe}@test.local";
            var creation = await admin.PostAsJsonAsync("/api/v1/Utilisateurs", new { nom, email, password = "Auteur123", role });
            Assert.Equal(HttpStatusCode.Created, creation.StatusCode);
            var id = (await Json(creation)).GetProperty("id").GetInt32();

            var connexion = await _api.ClientAnonyme().PostAsJsonAsync("/api/v1/Utilisateurs/authenticate", new { email, password = "Auteur123" });
            connexion.EnsureSuccessStatusCode();
            var client = _api.ClientAnonyme();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await Json(connexion)).GetProperty("token").GetString());
            return (client, id, nom);
        }

        [Fact]
        public async Task Suivi_EnregistreAuNomDuFermierConnecte()
        {
            var admin = await _api.ClientAsync("Admin");
            var (fermier, _, nom) = await NouveauCompteAsync("Fermier");
            var bande = await CreerBandeAsync(admin);

            var suivi = await fermier.PostAsJsonAsync("/api/v1/SuiviJournalier", new { bandeId = bande, jour = 1, dateSuivi = DateArrivee, nbMortalites = 0 });
            Assert.Equal(HttpStatusCode.Created, suivi.StatusCode);
            var id = (await Json(suivi)).GetProperty("id").GetInt32();

            var relu = await Json(await admin.GetAsync($"/api/v1/SuiviJournalier/{id}"));
            Assert.Equal(nom, relu.GetProperty("createurNom").GetString());
        }

        [Fact]
        public async Task Achat_EnregistreAuNomDuComptableConnecte()
        {
            var admin = await _api.ClientAsync("Admin");
            var (comptable, _, nom) = await NouveauCompteAsync("Comptable");
            var bande = await CreerBandeAsync(admin);
            var fournisseur = await CreerFournisseurAsync(admin);

            var achat = await comptable.PostAsJsonAsync("/api/v1/Achats", new
            {
                fournisseurId = fournisseur, bandeId = bande, dateAchat = DateTime.Today.AddDays(-3),
                typeAchat = "Aliment", quantite = 10, uniteMesure = "Kg", prixUnitaire = 300
            });
            Assert.True(achat.IsSuccessStatusCode, await achat.Content.ReadAsStringAsync());
            var id = (await Json(achat)).GetProperty("id").GetInt32();

            var relu = await Json(await admin.GetAsync($"/api/v1/Achats/{id}"));
            Assert.Equal(nom, relu.GetProperty("createurNom").GetString());
        }

        [Fact]
        public async Task Vente_EnregistreeAuNomDuComptableConnecte()
        {
            var admin = await _api.ClientAsync("Admin");
            var (comptable, _, nom) = await NouveauCompteAsync("Comptable");
            var bande = await CreerBandeAsync(admin);
            var client = await CreerClientAsync(admin);

            var vente = await comptable.PostAsJsonAsync("/api/v1/Ventes", new
            {
                bandeId = bande, clientId = client, dateVente = DateTime.Today, typeVente = "PouletsVivants",
                quantite = 2, uniteMesure = "Unite", prixUnitaire = 2500, modePaiement = "Especes", estPaye = false
            });
            Assert.True(vente.IsSuccessStatusCode, await vente.Content.ReadAsStringAsync());
            var id = (await Json(vente)).GetProperty("id").GetInt32();

            var relu = await Json(await admin.GetAsync($"/api/v1/Ventes/{id}"));
            Assert.Equal(nom, relu.GetProperty("createurNom").GetString());
        }

        [Fact]
        public async Task MarquerUneAlerte_AuNomDuConnecte_MemeSiLeCorpsDitAutreChose()
        {
            var admin = await _api.ClientAsync("Admin");
            var (fermier, fermierId, _) = await NouveauCompteAsync("Fermier");

            // Alerte « stock faible » : 50 kg, seuil 10 kg, 45 kg consommés
            var bande = await CreerBandeAsync(admin);
            var stock = await admin.PostAsJsonAsync("/api/v1/Stocks", new
            {
                bandeId = bande, type = "Aliment", nom = "Aliment auteur", quantiteInitiale = 50, uniteMesure = "Kg", seuilAlerte = 10
            });
            var stockId = (await Json(stock)).GetProperty("id").GetInt32();
            (await admin.PostAsJsonAsync($"/api/v1/Stocks/{stockId}/consommer", new { stockId, quantiteConsommee = 45 })).EnsureSuccessStatusCode();
            (await admin.PostAsync("/api/v1/Alertes/generer", null)).EnsureSuccessStatusCode();
            var alerteId = (await Json(await admin.GetAsync("/api/v1/Alertes/non-lues"))).EnumerateArray()
                .First(a => a.GetProperty("typeAlerte").GetString() == "StockFaible" && a.GetProperty("entiteId").GetInt32() == stockId)
                .GetProperty("id").GetInt32();

            // Le corps prétend que c'est l'utilisateur n° 1 : ignoré
            var marquage = await fermier.PostAsJsonAsync("/api/v1/Alertes/marquer", new { alerteIds = new[] { alerteId }, utilisateurId = 1, marquerToutes = false });
            Assert.Equal(HttpStatusCode.NoContent, marquage.StatusCode);

            var alerte = await Json(await admin.GetAsync($"/api/v1/Alertes/{alerteId}"));
            Assert.True(alerte.GetProperty("estLue").GetBoolean());
            Assert.Equal(fermierId, alerte.GetProperty("utilisateurId").GetInt32());
        }
    }
}
