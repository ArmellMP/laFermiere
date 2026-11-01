using System.Net;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Scénarios de bout en bout sur l'effectif d'une bande, qui bouge avec les ventes
    /// et les mortalités. Ils rejouent les bugs corrigés le 2026-11-01, sur une vraie base.
    /// </summary>
    [Collection("Api")]
    public class BandesVentesEtSuivisTests
    {
        private readonly ApiFixture _api;

        public BandesVentesEtSuivisTests(ApiFixture api)
        {
            _api = api;
        }

        /// <summary>
        /// Le scénario exact du bug du 500 sur DELETE /SuiviJournalier :
        /// une vente modifiée de 10 à 12 ne retirait que 10 poulets, sa suppression en rendait 12,
        /// puis la suppression du suivi rendait ses mortalités : l'effectif dépassait l'initial
        /// et la contrainte SQL chk_poussins_coherence refusait l'écriture (erreur 500).
        /// </summary>
        [Fact]
        public async Task VenteModifieePuisToutSupprime_EffectifRevientALInitial()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin, nbPoussins: 300);
            var client = await CreerClientAsync(admin);

            // Suivi : 3 mortalités => 297
            var suivi = await admin.PostAsJsonAsync("/api/v1/SuiviJournalier", new
            {
                bandeId = bande, jour = 1, dateSuivi = DateArrivee, nbMortalites = 3
            });
            Assert.Equal(HttpStatusCode.Created, suivi.StatusCode);
            var suiviId = (await Json(suivi)).GetProperty("id").GetInt32();

            // Vente de 10 => 287, puis modifiée à 12 => 285
            var vente = await admin.PostAsJsonAsync("/api/v1/Ventes", new
            {
                bandeId = bande, clientId = client, dateVente = DateTime.Today.AddDays(-1),
                typeVente = "PouletsVivants", quantite = 10, uniteMesure = "Unite", prixUnitaire = 2500,
                modePaiement = "Especes", estPaye = false
            });
            Assert.Equal(HttpStatusCode.Created, vente.StatusCode);
            var venteId = (await Json(vente)).GetProperty("id").GetInt32();

            var modif = await admin.PutAsJsonAsync($"/api/v1/Ventes/{venteId}", new { id = venteId, quantite = 12 });
            Assert.Equal(HttpStatusCode.OK, modif.StatusCode);
            Assert.Equal(30000m, (await Json(modif)).GetProperty("montantTotal").GetDecimal());
            Assert.Equal((300, 285), await EffectifAsync(admin, bande));

            // Tout supprimer : la vente rend 12 (297), le suivi rend 3 (300). Plus d'erreur 500.
            Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/v1/Ventes/{venteId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/v1/SuiviJournalier/{suiviId}")).StatusCode);
            Assert.Equal((300, 300), await EffectifAsync(admin, bande));
        }

        [Fact]
        public async Task StatistiquesDesSuivis_SansAucunSuivi_PasDErreur500()
        {
            var admin = await _api.ClientAsync("Admin");
            var bande = await CreerBandeAsync(admin);   // bande neuve : aucun suivi

            var reponse = await admin.GetAsync($"/api/v1/SuiviJournalier/stats?bandeId={bande}");

            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            Assert.Equal(0, (await Json(reponse)).GetProperty("totalSuivis").GetInt32());
        }

        /// <summary>
        /// Bug trouvé par ce lot de tests : le code automatique d'une bande valait toujours
        /// "AAAA-MM-JJ-001". Deux bandes arrivées le même jour => violation de l'unicité du code.
        /// </summary>
        [Fact]
        public async Task DeuxBandesSansCodeLeMemeJour_CodesDifferents()
        {
            var admin = await _api.ClientAsync("Admin");
            // Date propre à ce test, pour ne pas croiser les codes des autres tests
            var date = DateTime.Today.AddDays(-40);

            var premiere = await admin.PostAsJsonAsync("/api/v1/Bandes", new { dateArrivee = date, nbPoussinsInit = 50 });
            var seconde = await admin.PostAsJsonAsync("/api/v1/Bandes", new { dateArrivee = date, nbPoussinsInit = 60 });

            Assert.Equal(HttpStatusCode.Created, premiere.StatusCode);
            Assert.Equal(HttpStatusCode.Created, seconde.StatusCode);
            Assert.Equal($"{date:yyyy-MM-dd}-001", (await Json(premiere)).GetProperty("code").GetString());
            Assert.Equal($"{date:yyyy-MM-dd}-002", (await Json(seconde)).GetProperty("code").GetString());
        }
    }
}
