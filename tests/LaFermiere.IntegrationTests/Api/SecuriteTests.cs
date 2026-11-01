using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LaFermiere.IntegrationTests.Infrastructure;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Authentification (qui es-tu ?) et autorisation (as-tu le droit ?).
    ///   401 Unauthorized : pas de jeton, ou jeton invalide
    ///   403 Forbidden    : jeton valide, mais rôle non autorisé pour cette action
    /// Matrice des droits : voir DECISIONS.md (2026-10-31, "Protection par défaut + rôles").
    /// </summary>
    [Collection("Api")]
    public class SecuriteTests
    {
        private readonly ApiFixture _api;

        public SecuriteTests(ApiFixture api)
        {
            _api = api;
        }

        [Fact]
        public async Task Connexion_MauvaisMotDePasse_Refusee401()
        {
            var reponse = await _api.ClientAnonyme().PostAsJsonAsync("/api/v1/Utilisateurs/authenticate",
                new { email = ComptesDeTest.Admin.Email, password = "MauvaisMotDePasse1" });

            Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
        }

        [Fact]
        public async Task SansJeton_Refuse401()
        {
            var reponse = await _api.ClientAnonyme().GetAsync("/api/v1/Bandes");

            Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
        }

        [Fact]
        public async Task JetonInvente_Refuse401()
        {
            var client = _api.ClientAnonyme();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "pas.un.vrai.jeton");

            var reponse = await client.GetAsync("/api/v1/Bandes");

            Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
        }

        // Matrice des rôles, échantillonnée sur chaque règle de DECISIONS.md.
        // Seules des LECTURES sont utilisées : le test ne modifie aucune donnée,
        // et 200 / 403 dépend uniquement du rôle.
        [Theory]
        // Bandes, Stocks, Suivis : lecture pour tous
        [InlineData("Veterinaire", "/api/v1/Bandes", HttpStatusCode.OK)]
        [InlineData("Comptable", "/api/v1/Stocks", HttpStatusCode.OK)]
        [InlineData("Comptable", "/api/v1/SuiviJournalier/stats", HttpStatusCode.OK)]
        // Clients, Ventes, Fournisseurs : Admin et Comptable seulement
        [InlineData("Comptable", "/api/v1/Clients", HttpStatusCode.OK)]
        [InlineData("Fermier", "/api/v1/Clients", HttpStatusCode.Forbidden)]
        [InlineData("Veterinaire", "/api/v1/Ventes", HttpStatusCode.Forbidden)]
        [InlineData("Fermier", "/api/v1/Fournisseurs", HttpStatusCode.Forbidden)]
        // Achats : lecture Admin, Comptable, Fermier
        [InlineData("Fermier", "/api/v1/Achats", HttpStatusCode.OK)]
        [InlineData("Veterinaire", "/api/v1/Achats", HttpStatusCode.Forbidden)]
        // Alertes : lecture pour tous
        [InlineData("Veterinaire", "/api/v1/Alertes", HttpStatusCode.OK)]
        // Paramètres, Utilisateurs : Admin seulement
        [InlineData("Admin", "/api/v1/Parametres", HttpStatusCode.OK)]
        [InlineData("Comptable", "/api/v1/Parametres", HttpStatusCode.Forbidden)]
        [InlineData("Admin", "/api/v1/Utilisateurs", HttpStatusCode.OK)]
        [InlineData("Fermier", "/api/v1/Utilisateurs", HttpStatusCode.Forbidden)]
        public async Task Lecture_SelonLeRole(string role, string url, HttpStatusCode attendu)
        {
            var client = await _api.ClientAsync(role);

            var reponse = await client.GetAsync(url);

            Assert.Equal(attendu, reponse.StatusCode);
        }

        // Écritures interdites : l'autorisation est vérifiée AVANT la validation du corps,
        // donc un corps vide suffit, et rien n'est écrit.
        [Theory]
        [InlineData("Comptable", "/api/v1/Bandes")]          // écriture Bandes : Admin, Fermier
        [InlineData("Veterinaire", "/api/v1/Stocks")]        // écriture Stocks : Admin, Fermier
        [InlineData("Comptable", "/api/v1/SuiviJournalier")] // écriture Suivis : Admin, Fermier, Vétérinaire
        [InlineData("Fermier", "/api/v1/Achats")]            // écriture Achats : Admin, Comptable
        [InlineData("Fermier", "/api/v1/Alertes")]           // création d'alerte : Admin
        public async Task Ecriture_RoleNonAutorise_Refusee403(string role, string url)
        {
            var client = await _api.ClientAsync(role);

            var reponse = await client.PostAsJsonAsync(url, new { });

            Assert.Equal(HttpStatusCode.Forbidden, reponse.StatusCode);
        }

        [Fact]
        public async Task Ecriture_VeterinaireSurSuivi_Autorisee()
        {
            // Le vétérinaire PEUT écrire un suivi : ici le corps vide est refusé par la
            // validation (400), ce qui prouve que l'autorisation, elle, est passée.
            var client = await _api.ClientAsync("Veterinaire");

            var reponse = await client.PostAsJsonAsync("/api/v1/SuiviJournalier", new { });

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        }
    }
}
