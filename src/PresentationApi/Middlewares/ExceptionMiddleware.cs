// PresentationApi/Middlewares/ExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;
using FluentValidation;
using MySqlConnector;
using PresentationApi.Models.Responses;

namespace PresentationApi.Middlewares
{
    public class ExceptionMiddleware
    {
        // SQLSTATE '45000' = erreur "définie par l'utilisateur" en MySQL/MariaDB.
        // Nos triggers l'utilisent (SIGNAL SQLSTATE '45000') pour refuser une
        // opération contraire aux règles métier (stock insuffisant, unité invalide...).
        // Ce n'est donc pas une panne : c'est une erreur de la requête -> 400.
        private const string SqlStateRegleMetier = "45000";

        // Vrai si l'exception vient d'un SIGNAL '45000' d'un trigger
        private static bool EstRefusDeTrigger(Exception exception) =>
            exception is MySqlException mySqlException
            && mySqlException.SqlState == SqlStateRegleMetier;

        // Erreur MySQL 1062 "Duplicate entry 'x' for key 'Email'" : une contrainte UNIQUE
        // de la base a refusé la ligne. Ce n'est pas une panne mais un doublon -> 409.
        // Cas couverts : doublon non vérifié par le handler (ex. deux stocks du même type
        // pour une bande), ou deux requêtes simultanées qui passent toutes deux la vérification.
        private static bool EstDoublon(Exception exception) =>
            exception is MySqlException mySqlException
            && mySqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry;

        /// <summary>
        /// Message lisible selon la contrainte UNIQUE en cause (voir database/schema.sql).
        /// La valeur en double n'est pas recopiée dans la réponse.
        /// </summary>
        private static string MessageDoublon(Exception exception)
        {
            // MariaDB : "... for key 'Email'"  /  MySQL 8 : "... for key 'Utilisateurs.Email'"
            var cle = System.Text.RegularExpressions.Regex.Match(exception.Message, @"for key '([^']+)'").Groups[1].Value;
            cle = cle[(cle.LastIndexOf('.') + 1)..];

            return cle switch
            {
                "Email" => "Cette adresse e-mail est déjà utilisée par un autre utilisateur.",
                "Code" => "Une bande avec ce code existe déjà.",
                "Cle" => "Un paramètre avec cette clé existe déjà.",
                "idx_bande_type" => "Un stock de ce type existe déjà pour cette bande. Pour le réapprovisionner, enregistrez un achat.",
                "idx_bande_jour" => "Un suivi journalier existe déjà pour ce jour de la bande.",
                "idx_bande_date" => "Un suivi journalier existe déjà pour cette date.",
                _ => "Cette donnée existe déjà."
            };
        }

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Une erreur est survenue: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            // Même format que les erreurs DataAnnotations (voir Program.cs) : ErrorResponse
            var result = new ErrorResponse
            {
                Message = GetUserFriendlyMessage(exception),
                Errors = GetErrors(exception),
                TraceId = context.TraceIdentifier,
                StackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            };

            response.StatusCode = GetStatusCode(exception);
            await response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                NotFoundException => (int)HttpStatusCode.NotFound,
                ConflictException => (int)HttpStatusCode.Conflict,   // avant BusinessRuleException : elle en hérite
                BusinessRuleException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ when EstRefusDeTrigger(exception) => (int)HttpStatusCode.BadRequest,
                _ when EstDoublon(exception) => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ValidationException => ErrorResponse.MessageValidation,
                NotFoundException => exception.Message,
                BusinessRuleException => exception.Message,
                UnauthorizedAccessException => "Vous n'êtes pas autorisé à effectuer cette action.",
                // Le texte du SIGNAL (MESSAGE_TEXT) est écrit pour l'utilisateur : on le renvoie tel quel
                _ when EstRefusDeTrigger(exception) => exception.Message,
                _ when EstDoublon(exception) => MessageDoublon(exception),
                // Toute autre erreur : message générique, le détail reste dans les logs
                _ => "Une erreur technique est survenue. Veuillez réessayer plus tard."
            };
        }

        /// <summary>
        /// Erreurs FluentValidation regroupées par champ : { "nbPoussinsInit": ["...", "..."] }.
        /// null pour toute autre exception (pas d'erreurs par champ).
        /// </summary>
        private static Dictionary<string, string[]>? GetErrors(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                return validationException.Errors
                    // PropertyName vaut par ex. "CreateBandeDto.NbPoussinsInit" -> "nbPoussinsInit"
                    .GroupBy(e => ErrorResponse.NomDeChamp(e.PropertyName))
                    .ToDictionary(
                        groupe => groupe.Key,
                        groupe => groupe.Select(e => e.ErrorMessage).Distinct().ToArray());
            }

            return null;
        }
    }
}