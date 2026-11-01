using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Common.Behaviors
{
    /// <summary>
    /// Filtre MediatR qui valide chaque Command/Query AVANT son Handler.
    ///
    /// Sans lui, les classes AbstractValidator existent mais ne sont jamais appelées :
    /// FluentValidation ne s'exécute pas tout seul.
    ///
    /// Trajet d'une requête une fois ce filtre enregistré :
    ///   mediator.Send(command)
    ///     -> ValidationBehavior.Handle   (ici : lance les validateurs)
    ///         -> erreurs ?  oui : ValidationException, le Handler n'est PAS appelé
    ///                       non : next() appelle le Handler normalement
    ///
    /// L'exception est ensuite transformée en HTTP 400 par ExceptionMiddleware.
    /// </summary>
    /// <typeparam name="TRequest">Type de la Command/Query (ex. CreateBandeCommand)</typeparam>
    /// <typeparam name="TResponse">Type renvoyé par son Handler (ex. BandeResponseDto)</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        // Tous les validateurs enregistrés pour CE type de requête.
        // L'injection de dépendances fournit la liste (vide si la requête n'a pas de validateur,
        // c'est le cas de la plupart des Queries).
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Pas de validateur pour cette requête : on passe directement au Handler
            if (!_validators.Any())
            {
                return await next();
            }

            // Le contexte transporte la requête à valider vers chaque validateur
            var context = new ValidationContext<TRequest>(request);

            // Lancer tous les validateurs. Chacun renvoie un ValidationResult
            // contenant la liste de ses erreurs (vide si tout est correct).
            var results = new List<ValidationResult>();
            foreach (var validator in _validators)
            {
                results.Add(await validator.ValidateAsync(context, cancellationToken));
            }

            // Rassembler toutes les erreurs de tous les validateurs
            var failures = results
                .SelectMany(result => result.Errors)
                .Where(failure => failure != null)
                .ToList();

            // Au moins une erreur : on arrête tout, le Handler ne s'exécute pas
            // (donc aucune écriture en base avec des données invalides)
            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }

            // Tout est valide : on continue vers le Handler
            return await next();
        }
    }
}
