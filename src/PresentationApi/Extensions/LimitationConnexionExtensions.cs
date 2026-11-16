using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using PresentationApi.Models.Responses;

namespace PresentationApi.Extensions
{
    /// <summary>
    /// Réglages de la limitation des tentatives de connexion (section "LimitationConnexion"
    /// d'appsettings.json).
    /// </summary>
    public class LimitationConnexionOptions
    {
        public const string Section = "LimitationConnexion";

        /// <summary>Nombre de tentatives autorisées par adresse IP et par fenêtre. 0 = pas de limite.</summary>
        public int TentativesMax { get; set; } = 10;

        /// <summary>Durée de la fenêtre, en secondes. Le compteur repart à zéro à la fin de chaque fenêtre.</summary>
        public int FenetreSecondes { get; set; } = 60;
    }

    /// <summary>
    /// Limitation des tentatives de connexion (POST /api/v1/Utilisateurs/authenticate).
    ///
    /// Sans limite, un script peut essayer des milliers de mots de passe par minute sur un compte.
    /// Ici, chaque adresse IP a droit à TentativesMax essais par fenêtre de FenetreSecondes
    /// (10 par minute par défaut) ; au-delà, l'API répond 429 Too Many Requests, avec l'en-tête
    /// Retry-After (secondes à attendre) et le format d'erreur commun ErrorResponse.
    ///
    /// Le limiteur est celui intégré à ASP.NET Core (Microsoft.AspNetCore.RateLimiting) :
    /// aucun paquet à ajouter. Les compteurs sont en mémoire : ils repartent à zéro au
    /// redémarrage de l'API et ne sont pas partagés entre plusieurs instances.
    ///
    /// Derrière un reverse proxy (IIS, Nginx...), l'adresse vue par l'API est celle du proxy :
    /// il faudra alors activer UseForwardedHeaders pour retrouver l'IP réelle du client,
    /// sinon tous les utilisateurs partagent le même compteur.
    /// </summary>
    public static class LimitationConnexionExtensions
    {
        /// <summary>Nom de la politique, à placer sur l'action : [EnableRateLimiting(PolitiqueConnexion)].</summary>
        public const string PolitiqueConnexion = "Connexion";

        public static IServiceCollection AddLimitationConnexion(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LimitationConnexionOptions>(configuration.GetSection(LimitationConnexionOptions.Section));

            services.AddRateLimiter(options =>
            {
                // Un compteur par adresse IP. Les réglages sont relus à chaque requête (IOptions),
                // ce qui permet aux tests de les remplacer.
                options.AddPolicy(PolitiqueConnexion, contexte =>
                {
                    var reglages = contexte.RequestServices.GetRequiredService<IOptions<LimitationConnexionOptions>>().Value;
                    var adresseIp = contexte.Connection.RemoteIpAddress?.ToString() ?? "inconnue";

                    if (reglages.TentativesMax <= 0)
                    {
                        return RateLimitPartition.GetNoLimiter(adresseIp);
                    }

                    return RateLimitPartition.GetFixedWindowLimiter(adresseIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = reglages.TentativesMax,
                        Window = TimeSpan.FromSeconds(Math.Max(1, reglages.FenetreSecondes)),
                        QueueLimit = 0   // au-delà de la limite : refus immédiat, pas de file d'attente
                    });
                });

                options.OnRejected = async (contexte, jetonAnnulation) =>
                {
                    var reponse = contexte.HttpContext.Response;
                    var message = "Trop de tentatives de connexion. Réessayez plus tard.";

                    if (contexte.Lease.TryGetMetadata(MetadataName.RetryAfter, out var attente))
                    {
                        var secondes = (int)Math.Ceiling(attente.TotalSeconds);
                        reponse.Headers.RetryAfter = secondes.ToString();
                        message = $"Trop de tentatives de connexion. Réessayez dans {secondes} seconde(s).";
                    }

                    contexte.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(LimitationConnexionExtensions))
                        .LogWarning("Connexion refusée (trop de tentatives) pour l'adresse {AdresseIp}",
                            contexte.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnue");

                    reponse.StatusCode = StatusCodes.Status429TooManyRequests;
                    reponse.ContentType = "application/json";
                    var erreur = new ErrorResponse
                    {
                        Message = message,
                        TraceId = contexte.HttpContext.TraceIdentifier
                    };
                    await reponse.WriteAsync(JsonSerializer.Serialize(erreur, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }), jetonAnnulation);
                };
            });

            return services;
        }
    }
}
