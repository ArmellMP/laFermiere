using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LaFermiere.IntegrationTests.Infrastructure;
using static LaFermiere.IntegrationTests.Api.DonneesDeTest;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Sessions : jeton d'accès 15 min + refresh token 7 jours, rotation, détection de
    /// réutilisation, déconnexion et révocation lors d'un changement de compte.
    ///
    /// Chaque test crée son propre utilisateur : les comptes partagés de la fixture
    /// (dont les jetons sont gardés en cache) ne sont jamais déconnectés ni modifiés.
    /// </summary>
    [Collection("Api")]
    public class SessionsTests
    {
        private const string Base = "/api/v1/Utilisateurs";
        private const string MotDePasse = "MotDePasse1";

        private readonly ApiFixture _api;

        public SessionsTests(ApiFixture api)
        {
            _api = api;
        }

        private record Session(string Token, string RefreshToken, JsonElement Corps);

        /// <summary>Crée un utilisateur Fermier jetable et renvoie son id et son e-mail.</summary>
        private async Task<(int Id, string Email)> CreerUtilisateurAsync()
        {
            var admin = await _api.ClientAsync("Admin");
            var email = $"session{Guid.NewGuid():N}@test.local";
            var reponse = await admin.PostAsJsonAsync(Base, new { nom = "Session", email, password = MotDePasse, role = "Fermier" });
            Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
            return ((await Json(reponse)).GetProperty("id").GetInt32(), email);
        }

        private static Session LireSession(JsonElement corps) =>
            new(corps.GetProperty("token").GetString()!, corps.GetProperty("refreshToken").GetString()!, corps);

        private async Task<Session> ConnexionAsync(string email)
        {
            var reponse = await _api.ClientAnonyme().PostAsJsonAsync($"{Base}/authenticate", new { email, password = MotDePasse });
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            return LireSession(await Json(reponse));
        }

        private Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
            _api.ClientAnonyme().PostAsJsonAsync($"{Base}/refresh", new { refreshToken });

        private async Task<Session> RefreshReussiAsync(string refreshToken)
        {
            var reponse = await RefreshAsync(refreshToken);
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            return LireSession(await Json(reponse));
        }

        [Fact]
        public async Task Connexion_JetonDe15MinutesEtRefreshTokenDe7Jours()
        {
            var (_, email) = await CreerUtilisateurAsync();

            var session = await ConnexionAsync(email);

            var expiration = session.Corps.GetProperty("expiration").GetDateTime();
            var refreshExpiration = session.Corps.GetProperty("refreshExpiration").GetDateTime();
            Assert.InRange(expiration, DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
            Assert.InRange(refreshExpiration, DateTime.UtcNow.AddDays(7).AddMinutes(-1), DateTime.UtcNow.AddDays(7).AddMinutes(1));
            Assert.False(string.IsNullOrWhiteSpace(session.RefreshToken));
        }

        [Fact]
        public async Task Refresh_NouveauxJetonsUtilisables()
        {
            var (_, email) = await CreerUtilisateurAsync();
            var session = await ConnexionAsync(email);

            var nouvelle = await RefreshReussiAsync(session.RefreshToken);

            Assert.NotEqual(session.RefreshToken, nouvelle.RefreshToken);
            var client = _api.ClientAnonyme();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", nouvelle.Token);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/Bandes")).StatusCode);
        }

        [Fact]
        public async Task AncienRefreshTokenRejoue_401EtToute_La_SessionFermee()
        {
            var (_, email) = await CreerUtilisateurAsync();
            var t1 = (await ConnexionAsync(email)).RefreshToken;
            var t2 = (await RefreshReussiAsync(t1)).RefreshToken;

            // t1 a déjà servi : seul un voleur peut le présenter à nouveau
            Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(t1)).StatusCode);

            // Par précaution, le token légitime t2 est révoqué aussi : reconnexion obligatoire
            Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(t2)).StatusCode);
        }

        [Fact]
        public async Task Deconnexion_FermeLaSession_SansToucherAuxAutres()
        {
            var (_, email) = await CreerUtilisateurAsync();
            var telephone = await ConnexionAsync(email);
            var ordinateur = await ConnexionAsync(email);

            var reponse = await _api.ClientAnonyme().PostAsJsonAsync($"{Base}/logout", new { refreshToken = telephone.RefreshToken });

            Assert.Equal(HttpStatusCode.NoContent, reponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(telephone.RefreshToken)).StatusCode);
            await RefreshReussiAsync(ordinateur.RefreshToken);
        }

        [Fact]
        public async Task Deconnexion_TokenInconnu_204()
        {
            var reponse = await _api.ClientAnonyme().PostAsJsonAsync($"{Base}/logout", new { refreshToken = "inconnu" });

            Assert.Equal(HttpStatusCode.NoContent, reponse.StatusCode);
        }

        [Fact]
        public async Task Refresh_TokenInconnu_401AuFormatAuthResponse()
        {
            var reponse = await RefreshAsync("inconnu");

            Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
            var corps = await Json(reponse);
            Assert.False(corps.GetProperty("success").GetBoolean());
            Assert.Equal("Session expirée ou invalide. Veuillez vous reconnecter.", corps.GetProperty("message").GetString());
        }

        [Theory]
        [InlineData("motDePasse")]
        [InlineData("role")]
        [InlineData("desactivation")]
        public async Task ChangementDeCompte_FermeToutesLesSessions(string changement)
        {
            var (id, email) = await CreerUtilisateurAsync();
            var session = await ConnexionAsync(email);
            var admin = await _api.ClientAsync("Admin");

            object modification = changement switch
            {
                "motDePasse" => new { id, password = "NouveauMotDePasse1" },
                "role" => new { id, role = "Comptable" },
                _ => new { id, estActif = false }
            };
            Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"{Base}/{id}", modification)).StatusCode);

            Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(session.RefreshToken)).StatusCode);
        }

        [Fact]
        public async Task ChangementDeNom_NeFermePasLesSessions()
        {
            var (id, email) = await CreerUtilisateurAsync();
            var session = await ConnexionAsync(email);
            var admin = await _api.ClientAsync("Admin");

            Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"{Base}/{id}", new { id, nom = "Nouveau nom" })).StatusCode);

            await RefreshReussiAsync(session.RefreshToken);
        }
    }
}
