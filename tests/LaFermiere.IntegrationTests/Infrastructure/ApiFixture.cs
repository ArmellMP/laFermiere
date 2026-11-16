using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LaFermiere.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Démarre la VRAIE API, en mémoire, branchée sur la base lafermiere_tests.
    ///
    /// WebApplicationFactory&lt;Program&gt; (paquet Microsoft.AspNetCore.Mvc.Testing) exécute
    /// Program.cs comme au lancement normal (contrôleurs, JWT, middleware, MediatR, Dapper...),
    /// mais sans ouvrir de port réseau : les requêtes HTTP passent directement en mémoire.
    /// Les tests vérifient donc tout le trajet, du contrôleur jusqu'aux triggers MySQL.
    ///
    /// "Fixture" (xUnit) : objet créé UNE seule fois et partagé par tous les tests de la
    /// collection "Api". La base n'est recréée qu'une fois par lancement : c'est plus rapide,
    /// et chaque test crée ses propres données (sa bande, son fournisseur...) pour rester
    /// indépendant des autres.
    ///
    /// IAsyncLifetime : xUnit appelle InitializeAsync avant le premier test, DisposeAsync après le dernier.
    /// </summary>
    public sealed class ApiFixture : IAsyncLifetime
    {
        private WebApplicationFactory<Program>? _factory;
        private readonly Dictionary<string, string> _jetons = new();

        // Clé JWT propre aux tests : l'API de test ne dépend pas de la clé du poste de travail
        private const string CleJwtDeTest = "cle-de-test-uniquement-pour-les-tests-integration-lafermiere-2026";

        public async ValueTask InitializeAsync()
        {
            // 1. Base propre
            await BaseDeTest.RecreerAsync();

            // 2. Configuration de l'API pendant les tests, par variables d'environnement.
            //    Dans ASP.NET Core, les variables d'environnement sont lues APRÈS appsettings.json
            //    et les user-secrets : elles ont donc la priorité.
            //    "__" (deux tirets bas) = séparateur de section : ConnectionStrings:DefaultConnection.
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", BaseDeTest.ChaineDeConnexion());
            Environment.SetEnvironmentVariable("JwtSettings__SecretKey", CleJwtDeTest);

            // 3. L'API démarre au premier CreateClient()
            _factory = new WebApplicationFactory<Program>();
        }

        public async ValueTask DisposeAsync()
        {
            if (_factory != null)
            {
                await _factory.DisposeAsync();
            }
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
            Environment.SetEnvironmentVariable("JwtSettings__SecretKey", null);
        }

        /// <summary>Client HTTP sans jeton (utilisateur non connecté).</summary>
        public HttpClient ClientAnonyme() =>
            (_factory ?? throw new InvalidOperationException("Fixture non initialisée")).CreateClient();

        /// <summary>
        /// Client HTTP connecté avec le compte de test du rôle demandé ("Admin", "Fermier"...).
        /// Le jeton est obtenu par la vraie route /authenticate, puis gardé pour les tests suivants.
        /// </summary>
        public async Task<HttpClient> ClientAsync(string role)
        {
            if (!_jetons.TryGetValue(role, out var jeton))
            {
                var compte = ComptesDeTest.ParRole(role);
                var reponse = await ClientAnonyme().PostAsJsonAsync(
                    "/api/v1/Utilisateurs/authenticate", new { email = compte.Email, password = compte.MotDePasse });
                reponse.EnsureSuccessStatusCode();

                var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();
                jeton = corps.GetProperty("token").GetString()!;
                _jetons[role] = jeton;
            }

            var client = ClientAnonyme();
            // En-tête "Authorization: Bearer <jeton>" ajouté à toutes les requêtes de ce client
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jeton);
            return client;
        }
    }

    /// <summary>
    /// Collection "Api" : tous les tests marqués [Collection("Api")] partagent la même ApiFixture
    /// (une seule base, une seule API) et s'exécutent les uns après les autres.
    /// </summary>
    [CollectionDefinition("Api")]
    public class ApiCollection : ICollectionFixture<ApiFixture>
    {
    }
}
