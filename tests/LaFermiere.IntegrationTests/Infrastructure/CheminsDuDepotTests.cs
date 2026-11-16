namespace LaFermiere.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Premier test d'intégration, volontairement simple : il vérifie que les tests
    /// retrouvent le script database/schema.sql du dépôt.
    ///
    /// Pourquoi : au lot 4, la base de test lafermiere_tests sera recréée à partir de ce
    /// script avant chaque lancement. Si le chemin est faux, autant le savoir tout de suite.
    /// </summary>
    public class CheminsDuDepotTests
    {
        [Fact]
        public void SchemaSql_EstTrouveDepuisLeDossierDesTests()
        {
            // Act
            var chemin = CheminsDuDepot.SchemaSql();

            // Assert : le fichier existe et contient bien la création des tables
            Assert.True(File.Exists(chemin), $"Fichier introuvable : {chemin}");
            Assert.Contains("CREATE TABLE Bandes", File.ReadAllText(chemin));
        }
    }

    /// <summary>
    /// Retrouve les fichiers du dépôt depuis le dossier où les tests s'exécutent.
    /// Les tests tournent dans tests/.../bin/Debug/net10.0 : on remonte les dossiers parents
    /// jusqu'à trouver celui qui contient la solution laFermiere.slnx (la racine du dépôt).
    /// Ainsi, le chemin marche sur n'importe quel PC, quel que soit l'emplacement du clone.
    /// </summary>
    public static class CheminsDuDepot
    {
        public static string Racine()
        {
            var dossier = new DirectoryInfo(AppContext.BaseDirectory);

            while (dossier != null && !File.Exists(Path.Combine(dossier.FullName, "laFermiere.slnx")))
            {
                dossier = dossier.Parent;
            }

            return dossier?.FullName
                ?? throw new InvalidOperationException("Racine du dépôt (laFermiere.slnx) introuvable.");
        }

        public static string SchemaSql() => Path.Combine(Racine(), "database", "schema.sql");
    }
}
