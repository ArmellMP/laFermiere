using System.Net;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Règles de suppression et de clôture d'une bande :
    ///   suppression : seule une bande vide (sans suivi, stock, vente ni achat) peut être supprimée ;
    ///   clôture     : refusée s'il manque un suivi journalier entre le jour 1 et la veille ;
    ///                 les stocks non consommés sont notés dans les observations (pas de blocage).
    /// </summary>
    [Collection("Api")]
    public class BandesReglesMetierTests
    {
        // Date propre aux bandes à clôturer : 35 jours => cycle valide (minimum 30 jours)
        private static readonly DateTime ArriveeCloture = DateTime.Today.AddDays(-35);

        private readonly ApiFixture _api;

        public BandesReglesMetierTests(ApiFixture api)
        {
            _api = api;
        }

        // ============================================
        // SUPPRESSION
        // ============================================

        [Fact]
        public async Task SupprimerBandeVide_204()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);

            Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/v1/Bandes/{bande}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/v1/Bandes/{bande}")).StatusCode);
        }

        /// <summary>
        /// Avant : la base effaçait en silence les suivis et les stocks (ON DELETE CASCADE).
        /// </summary>
        [Fact]
        public async Task SupprimerBandeAvecSuiviEtStock_400AvecDecompte_RienNEstEfface()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            await CreerSuivisAsync(admin, bande, DateArrivee, 1);
            await CreerStockAsync(admin, bande, 25);

            var reponse = await admin.DeleteAsync($"/api/v1/Bandes/{bande}");

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            var message = (await Json(reponse)).GetProperty("message").GetString();
            Assert.Contains("1 suivi(s) journalier(s)", message);
            Assert.Contains("1 stock(s)", message);
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/v1/Bandes/{bande}")).StatusCode);
            Assert.NotNull(await StockAsync(admin, bande, "Aliment"));
        }

        /// <summary>
        /// Avant : la clé étrangère de Ventes refusait la suppression => erreur 500.
        /// </summary>
        [Fact]
        public async Task SupprimerBandeAvecVente_400EtPlus500()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);
            var client = await CreerClientAsync(admin);
            var vente = await admin.PostAsJsonAsync("/api/v1/Ventes", new
            {
                bandeId = bande, clientId = client, dateVente = DateTime.Today.AddDays(-1),
                typeVente = "PouletsVivants", quantite = 5, uniteMesure = "Unite", prixUnitaire = 2500,
                modePaiement = "Especes", estPaye = false
            });
            Assert.Equal(HttpStatusCode.Created, vente.StatusCode);

            var reponse = await admin.DeleteAsync($"/api/v1/Bandes/{bande}");

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            Assert.Contains("1 vente(s)", (await Json(reponse)).GetProperty("message").GetString());
        }

        // ============================================
        // CLÔTURE
        // ============================================

        [Fact]
        public async Task CloturerAvecSuivisManquants_400ListeLesJours()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeEnCoursAsync(admin);
            // Jours 1 à 20 et 25 saisis : il manque 21-24 et 26-35
            await CreerSuivisAsync(admin, bande, ArriveeCloture, Enumerable.Range(1, 20).Append(25).ToArray());

            var reponse = await admin.PostAsJsonAsync($"/api/v1/Bandes/{bande}/cloturer", new { id = bande });

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
            var message = (await Json(reponse)).GetProperty("message").GetString();
            Assert.Contains("14 suivi(s) journalier(s) manquant(s)", message);
            Assert.Contains("21-24, 26-35", message);
        }

        [Fact]
        public async Task CloturerAvecTousLesSuivis_200EtStockRestantNote()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeEnCoursAsync(admin);
            await CreerStockAsync(admin, bande, 12.5m);
            await CreerSuivisAsync(admin, bande, ArriveeCloture, Enumerable.Range(1, 35).ToArray());

            var reponse = await admin.PostAsJsonAsync($"/api/v1/Bandes/{bande}/cloturer",
                new { id = bande, observationsCloture = "Fin de cycle" });

            var corps = await Json(reponse);
            Assert.True(reponse.StatusCode == HttpStatusCode.OK, $"Clôture refusée : {corps}");
            Assert.Equal("Cloturee", corps.GetProperty("statut").GetString());
            var observations = corps.GetProperty("observations").GetString();
            Assert.Contains("Observations: Fin de cycle", observations);
            Assert.Contains("Stocks non consommés: Aliment test", observations);
        }

        // ============================================
        // OUTILS
        // ============================================

        /// <summary>Bande arrivée il y a 35 jours, passée au statut "EnCours".</summary>
        private static async Task<int> CreerBandeEnCoursAsync(HttpClient admin)
        {
            var creation = await admin.PostAsJsonAsync("/api/v1/Bandes",
                new { dateArrivee = ArriveeCloture, nbPoussinsInit = 200 });
            Assert.Equal(HttpStatusCode.Created, creation.StatusCode);
            var id = (await Json(creation)).GetProperty("id").GetInt32();

            var enCours = await admin.PutAsJsonAsync($"/api/v1/Bandes/{id}",
                new { id, nbPoussinsActuel = 200, statut = "EnCours" });
            Assert.Equal(HttpStatusCode.OK, enCours.StatusCode);
            return id;
        }

        /// <summary>Un suivi sans mortalité pour chaque jour demandé (jour 1 = jour d'arrivée).</summary>
        private static async Task CreerSuivisAsync(HttpClient admin, int bandeId, DateTime arrivee, params int[] jours)
        {
            foreach (var jour in jours)
            {
                var reponse = await admin.PostAsJsonAsync("/api/v1/SuiviJournalier", new
                {
                    bandeId, jour, dateSuivi = arrivee.AddDays(jour - 1), nbMortalites = 0
                });
                Assert.True(reponse.StatusCode == HttpStatusCode.Created,
                    $"Suivi jour {jour} refusé : {await reponse.Content.ReadAsStringAsync()}");
            }
        }

        private static async Task CreerStockAsync(HttpClient admin, int bandeId, decimal quantite)
        {
            var reponse = await admin.PostAsJsonAsync("/api/v1/Stocks", new
            {
                bandeId, type = "Aliment", nom = "Aliment test", quantiteInitiale = quantite, uniteMesure = "Kg"
            });
            Assert.True(reponse.StatusCode == HttpStatusCode.Created,
                $"Stock refusé : {await reponse.Content.ReadAsStringAsync()}");
        }
    }
}
