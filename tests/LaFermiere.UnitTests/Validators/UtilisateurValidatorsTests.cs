using Application.Features.Utilisateurs.Dtos;
using Application.Features.Utilisateurs.Validators;
using FluentValidation.TestHelper;

namespace LaFermiere.UnitTests.Validators
{
    /// <summary>
    /// Tests de la règle du mot de passe (décision n°9, DECISIONS.md, 2026-11-01) :
    /// 6 caractères minimum, au moins une majuscule, une minuscule et un chiffre.
    /// </summary>
    public class UtilisateurValidatorsTests
    {
        private readonly CreateUtilisateurDtoValidator _validator = new();

        private static CreateUtilisateurDto UtilisateurAvecMotDePasse(string motDePasse) => new()
        {
            Nom = "Jean Test",
            Email = "jean.test@ferme.local",
            Password = motDePasse,
            Role = "Fermier"
        };

        // Les mots de passe des comptes de test respectent la règle :
        // si ce test échouait, plus personne ne pourrait recréer ces comptes.
        [Theory]
        [InlineData("Admin123")]
        [InlineData("Fermier123")]
        [InlineData("Veto1234")]
        [InlineData("Compta123")]
        [InlineData("Abc123")]      // exactement 6 caractères : borne acceptée
        public void MotDePasse_Conforme_Accepte(string motDePasse)
        {
            var resultat = _validator.TestValidate(UtilisateurAvecMotDePasse(motDePasse));

            resultat.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Theory]
        [InlineData("abcdef12")]   // pas de majuscule
        [InlineData("ABCDEF12")]   // pas de minuscule
        [InlineData("Abcdefgh")]   // pas de chiffre
        [InlineData("Ab12")]       // trop court (4 caractères)
        [InlineData("")]           // vide
        public void MotDePasse_NonConforme_Refuse(string motDePasse)
        {
            var resultat = _validator.TestValidate(UtilisateurAvecMotDePasse(motDePasse));

            resultat.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
