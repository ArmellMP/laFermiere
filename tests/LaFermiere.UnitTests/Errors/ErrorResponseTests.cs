using PresentationApi.Models.Responses;

namespace LaFermiere.UnitTests.Errors
{
    /// <summary>
    /// Premiers tests unitaires : ErrorResponse.NomDeChamp.
    ///
    /// Pourquoi commencer par elle : c'est une fonction "pure" (même entrée => même sortie,
    /// aucune base de données, aucun service). C'est le cas le plus simple à tester.
    ///
    /// Convention de nommage des tests : Methode_Situation_ResultatAttendu.
    /// Structure de chaque test, dite "AAA" :
    ///   Arrange : préparer les données
    ///   Act     : appeler le code testé
    ///   Assert  : vérifier le résultat
    /// </summary>
    public class ErrorResponseTests
    {
        // [Fact] = un test unique, sans paramètre.
        [Fact]
        public void NomDeChamp_NomSimple_PremiereLettreEnMinuscule()
        {
            // Arrange
            var nomTechnique = "NbPoussinsInit";

            // Act
            var resultat = ErrorResponse.NomDeChamp(nomTechnique);

            // Assert : Assert.Equal(attendu, obtenu). Si les deux diffèrent, le test échoue
            // et xUnit affiche les deux valeurs.
            Assert.Equal("nbPoussinsInit", resultat);
        }

        // [Theory] = le même test joué plusieurs fois.
        // Chaque [InlineData(...)] fournit un jeu de paramètres (ici : entrée, sortie attendue).
        // xUnit compte un test par InlineData : 6 tests ici.
        [Theory]
        [InlineData("CreateBandeDto.NbPoussinsInit", "nbPoussinsInit")] // FluentValidation via SetValidator
        [InlineData("$.nbPoussinsInit", "nbPoussinsInit")]              // JSON mal formé
        [InlineData("Email", "email")]                                  // DataAnnotations
        [InlineData("", "body")]                                        // règle sur l'objet entier
        [InlineData("$", "body")]                                       // JSON illisible
        [InlineData("CreateSuiviDto", "body")]                          // règle sur l'objet d'un validateur imbriqué
        public void NomDeChamp_NomTechnique_DonneLeNomDuChampJson(string nomTechnique, string attendu)
        {
            var resultat = ErrorResponse.NomDeChamp(nomTechnique);

            Assert.Equal(attendu, resultat);
        }
    }
}
