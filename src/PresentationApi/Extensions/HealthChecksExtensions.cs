using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PresentationApi.Extensions
{
    /// <summary>
    /// Health checks : routes que la supervision (hébergeur, Docker, outil de monitoring)
    /// appelle pour savoir si l'API fonctionne.
    ///
    ///   GET /health       → l'API ET la base MySQL répondent (200 "Healthy", sinon 503 "Unhealthy")
    ///   GET /health/live  → le processus de l'API tourne (200, sans toucher à la base)
    ///
    /// Les deux routes sont publiques (pas de jeton) : un outil de supervision n'est pas connecté.
    /// Elles ne renvoient que des statuts et des durées ; le détail d'une erreur (message
    /// MySQL, nom du serveur...) n'est ajouté qu'en environnement Development.
    /// </summary>
    public static class HealthChecksExtensions
    {
        /// <summary>Étiquette des vérifications qui dépendent d'un service externe (la base).</summary>
        private const string TagDependances = "dependances";

        public static IServiceCollection AddLaFermiereHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                // Ouvre une connexion et exécute "SELECT 1" sur la base de l'application.
                // La chaîne est lue à l'exécution (et non à l'enregistrement) : c'est celle
                // que l'API utilise réellement, y compris quand les tests la remplacent.
                .AddMySql(
                    sp => sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
                    name: "mysql",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { TagDependances },
                    // Base injoignable : réponse 503 en 5 secondes au plus, au lieu d'attendre
                    // le délai de connexion par défaut
                    timeout: TimeSpan.FromSeconds(5));

            return services;
        }

        public static WebApplication MapLaFermiereHealthChecks(this WebApplication app)
        {
            var afficherErreurs = app.Environment.IsDevelopment();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = (contexte, rapport) => EcrireRapportAsync(contexte, rapport, afficherErreurs)
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                // Aucune vérification : répondre suffit à prouver que le processus tourne
                Predicate = _ => false,
                ResponseWriter = (contexte, rapport) => EcrireRapportAsync(contexte, rapport, afficherErreurs)
            });

            return app;
        }

        /// <summary>
        /// Réponse JSON lisible au lieu du texte brut "Healthy" par défaut :
        /// { "statut": "Healthy", "dureeMs": 12, "verifications": [ { "nom": "mysql", "statut": "Healthy", "dureeMs": 11 } ] }
        /// Le code HTTP (200 / 503) est fixé par ASP.NET Core selon le statut global.
        /// </summary>
        private static Task EcrireRapportAsync(HttpContext contexte, HealthReport rapport, bool afficherErreurs)
        {
            var corps = new
            {
                statut = rapport.Status.ToString(),
                dureeMs = Math.Round(rapport.TotalDuration.TotalMilliseconds),
                verifications = rapport.Entries.Select(e => new
                {
                    nom = e.Key,
                    statut = e.Value.Status.ToString(),
                    dureeMs = Math.Round(e.Value.Duration.TotalMilliseconds),
                    erreur = afficherErreurs ? (e.Value.Exception?.Message ?? e.Value.Description) : null
                })
            };

            contexte.Response.ContentType = "application/json; charset=utf-8";
            return contexte.Response.WriteAsync(JsonSerializer.Serialize(corps, OptionsJson));
        }

        private static readonly JsonSerializerOptions OptionsJson = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
}
