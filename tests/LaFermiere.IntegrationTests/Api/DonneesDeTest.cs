using System.Net.Http.Json;
using System.Text.Json;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Raccourcis pour créer les données dont un test a besoin, en passant par l'API
    /// (comme le ferait un vrai utilisateur). Chaque test crée SES données : il ne dépend
    /// ni de l'ordre d'exécution, ni de ce qu'ont fait les autres tests.
    /// </summary>
    public static class DonneesDeTest
    {
        // Compteur partagé : donne un code de bande unique à chaque appel (001, 002...)
        private static int _numeroBande;

        // Date d'arrivée commune aux bandes créées ici (passée, donc acceptée par la validation)
        public static readonly DateTime DateArrivee = DateTime.Today.AddDays(-20);

        /// <summary>Lit le corps JSON d'une réponse (propriétés en camelCase).</summary>
        public static async Task<JsonElement> Json(HttpResponseMessage reponse) =>
            await reponse.Content.ReadFromJsonAsync<JsonElement>();

        public static async Task<int> CreerBandeAsync(HttpClient admin, int nbPoussins = 100)
        {
            // Code explicite et unique : ces tests ne dépendent pas de la génération automatique
            // (testée à part dans BandesTests)
            var numero = Interlocked.Increment(ref _numeroBande);
            var code = $"{DateArrivee:yyyy-MM-dd}-{numero:000}";

            var reponse = await admin.PostAsJsonAsync("/api/v1/Bandes",
                new { dateArrivee = DateArrivee, nbPoussinsInit = nbPoussins, observations = (string?)null, code });
            return await IdCree(reponse, "bande");
        }

        public static async Task<int> CreerFournisseurAsync(HttpClient admin)
        {
            var reponse = await admin.PostAsJsonAsync("/api/v1/Fournisseurs",
                new { nom = "Fournisseur test", typeFournisseur = "Aliment", telephone = "690000001" });
            return await IdCree(reponse, "fournisseur");
        }

        public static async Task<int> CreerClientAsync(HttpClient admin)
        {
            var reponse = await admin.PostAsJsonAsync("/api/v1/Clients",
                new { nom = "Client test", typeClient = "Restaurant", telephone = "690000002" });
            return await IdCree(reponse, "client");
        }

        public static async Task<(int Init, int Actuel)> EffectifAsync(HttpClient client, int bandeId)
        {
            var bande = await Json(await client.GetAsync($"/api/v1/Bandes/{bandeId}"));
            return (bande.GetProperty("nbPoussinsInit").GetInt32(), bande.GetProperty("nbPoussinsActuel").GetInt32());
        }

        /// <summary>Quantités (initiale, actuelle) du stock d'un type pour une bande ; null s'il n'existe pas.</summary>
        public static async Task<(decimal Initiale, decimal Actuelle)?> StockAsync(HttpClient client, int bandeId, string type)
        {
            var stocks = await Json(await client.GetAsync($"/api/v1/Stocks/bande/{bandeId}"));
            foreach (var stock in stocks.EnumerateArray())
            {
                if (stock.GetProperty("type").GetString() == type)
                {
                    return (stock.GetProperty("quantiteInitiale").GetDecimal(), stock.GetProperty("quantiteActuelle").GetDecimal());
                }
            }
            return null;
        }

        // Vérifie la création (201) et renvoie l'id ; sinon, message clair avec la réponse de l'API
        private static async Task<int> IdCree(HttpResponseMessage reponse, string quoi)
        {
            var corps = await reponse.Content.ReadAsStringAsync();
            Assert.True(reponse.IsSuccessStatusCode, $"Création de {quoi} refusée ({(int)reponse.StatusCode}) : {corps}");
            return JsonDocument.Parse(corps).RootElement.GetProperty("id").GetInt32();
        }
    }
}
