namespace PresentationApi.Models.Responses
{
    /// <summary>
    /// Format UNIQUE de toutes les réponses d'erreur de l'API (400, 401, 404, 500).
    ///
    /// Avant, deux formats coexistaient pour une erreur de validation :
    ///   - DataAnnotations ([Required], [Range]) : format "ProblemDetails" d'ASP.NET
    ///   - FluentValidation (via ExceptionMiddleware) : liste de messages sans nom de champ
    /// Un client (ex. le futur frontend React) n'a plus qu'un seul format à lire :
    ///
    ///   {
    ///     "success": false,
    ///     "message": "Les données envoyées ne sont pas valides.",
    ///     "errors": { "nbPoussinsInit": ["Le nombre de poussins doit être entre 1 et 50 000"] },
    ///     "traceId": "0HN..."
    ///   }
    ///
    /// "errors" n'est rempli que pour les erreurs de validation : une liste de messages par champ,
    /// avec le nom du champ tel qu'il apparaît dans le JSON envoyé (camelCase).
    /// </summary>
    public class ErrorResponse
    {
        public bool Success { get; init; } = false;

        /// <summary>Message général, lisible par l'utilisateur.</summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>Erreurs de validation par champ (null pour les autres erreurs).</summary>
        public Dictionary<string, string[]>? Errors { get; init; }

        /// <summary>Identifiant de la requête : permet de retrouver l'erreur dans les logs.</summary>
        public string TraceId { get; init; } = string.Empty;

        /// <summary>Pile d'appels : renseignée uniquement en Development (jamais en production).</summary>
        public string? StackTrace { get; init; }

        /// <summary>Message commun à toutes les erreurs de validation.</summary>
        public const string MessageValidation = "Les données envoyées ne sont pas valides.";

        /// <summary>
        /// Transforme un nom de champ technique en nom de champ JSON (camelCase, sans préfixe).
        /// Exemples :
        ///   "CreateBandeDto.NbPoussinsInit" (FluentValidation, via SetValidator) -> "nbPoussinsInit"
        ///   "NbPoussinsInit"                (DataAnnotations)                     -> "nbPoussinsInit"
        ///   "$.nbPoussinsInit"              (JSON mal formé)                      -> "nbPoussinsInit"
        ///   "" ou "CreateSuiviDto"          (règle sur l'objet entier)            -> "body"
        /// </summary>
        public static string NomDeChamp(string nomTechnique)
        {
            // On garde le dernier segment après le dernier point : le nom de la propriété
            var nom = nomTechnique.Contains('.')
                ? nomTechnique[(nomTechnique.LastIndexOf('.') + 1)..]
                : nomTechnique;

            // Erreur qui porte sur l'objet ENTIER, pas sur un champ : clé générique "body".
            //   ""      : règle FluentValidation sur l'objet (ex. "unité cohérente avec le type")
            //   "$"     : JSON illisible
            //   "...Dto": règle sur l'objet d'un validateur imbriqué (SetValidator), qui reçoit
            //             le nom de la propriété de la Command (ex. "CreateSuiviDto")
            if (string.IsNullOrWhiteSpace(nom) || nom == "$" || nom.EndsWith("Dto"))
            {
                return "body";
            }

            // Première lettre en minuscule : NbPoussinsInit -> nbPoussinsInit
            return char.ToLowerInvariant(nom[0]) + nom[1..];
        }
    }
}
