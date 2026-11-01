using Application.Common.Behaviors;
using FluentValidation;
using MediatR;

namespace LaFermiere.UnitTests.Behaviors
{
    /// <summary>
    /// Tests du ValidationBehavior : le filtre MediatR qui lance les validateurs
    /// avant chaque Handler.
    ///
    /// Garantie testée : une requête invalide N'ATTEINT JAMAIS le Handler
    /// (donc aucune écriture en base avec des données invalides).
    ///
    /// Pour isoler le filtre, on n'utilise pas les vraies Commands du projet :
    /// on crée une requête et un validateur minuscules, propres à ce test.
    /// Le "Handler" est remplacé par une fonction qui note si elle a été appelée.
    /// </summary>
    public class ValidationBehaviorTests
    {
        // Requête de test : un seul champ, qui doit être positif
        public record DemandeTest(int Valeur) : IRequest<string>;

        public class DemandeTestValidator : AbstractValidator<DemandeTest>
        {
            public DemandeTestValidator()
            {
                RuleFor(x => x.Valeur).GreaterThan(0).WithMessage("La valeur doit être positive.");
            }
        }

        // Faux Handler : retient s'il a été appelé et renvoie "résultat du handler"
        private bool _handlerAppele;

        private Task<string> FauxHandler()
        {
            _handlerAppele = true;
            return Task.FromResult("résultat du handler");
        }

        [Fact]
        public async Task RequeteValide_AppelleLeHandlerEtRenvoieSonResultat()
        {
            // Arrange : le filtre reçoit la liste des validateurs (ici, un seul)
            var behavior = new ValidationBehavior<DemandeTest, string>(new[] { new DemandeTestValidator() });

            // Act : "FauxHandler" joue le rôle de next(), l'étape suivante du pipeline
            var resultat = await behavior.Handle(new DemandeTest(5), FauxHandler, CancellationToken.None);

            // Assert
            Assert.True(_handlerAppele);
            Assert.Equal("résultat du handler", resultat);
        }

        [Fact]
        public async Task RequeteInvalide_LeveValidationExceptionSansAppelerLeHandler()
        {
            var behavior = new ValidationBehavior<DemandeTest, string>(new[] { new DemandeTestValidator() });

            // Assert.ThrowsAsync vérifie que le code lève bien CETTE exception,
            // et la renvoie pour qu'on puisse examiner son contenu.
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => behavior.Handle(new DemandeTest(-1), FauxHandler, CancellationToken.None));

            Assert.False(_handlerAppele);   // le point essentiel : le Handler n'a pas tourné
            var erreur = Assert.Single(exception.Errors);   // exactement une erreur...
            Assert.Equal("Valeur", erreur.PropertyName);    // ...sur le bon champ
        }

        [Fact]
        public async Task SansValidateur_AppelleDirectementLeHandler()
        {
            // Cas de la plupart des Queries : aucun validateur enregistré
            var behavior = new ValidationBehavior<DemandeTest, string>(Array.Empty<IValidator<DemandeTest>>());

            var resultat = await behavior.Handle(new DemandeTest(-1), FauxHandler, CancellationToken.None);

            Assert.True(_handlerAppele);
            Assert.Equal("résultat du handler", resultat);
        }
    }
}
