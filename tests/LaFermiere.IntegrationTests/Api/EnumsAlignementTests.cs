using Domain.Enums;
using LaFermiere.IntegrationTests.Infrastructure;
using MySqlConnector;

namespace LaFermiere.IntegrationTests.Api
{
    /// <summary>
    /// Les enums C# (Domain/Enums) doivent avoir EXACTEMENT les mêmes valeurs que les
    /// colonnes ENUM de la base : le code écrit enum.ToString() en base, et relit la valeur
    /// avec Enum.Parse. Une valeur présente d'un seul côté provoque une erreur à l'écriture
    /// ou à la lecture. (Test prévu depuis l'audit des enums du 2026-10-31.)
    ///
    /// La base de test est créée depuis schema.sql : ce test vérifie donc aussi que
    /// schema.sql est à jour.
    /// </summary>
    [Collection("Api")]
    public class EnumsAlignementTests
    {
        // Les 15 colonnes ENUM de la base et l'enum C# correspondant.
        // TheoryData : données typées pour un [Theory] (ici, des Type C# impossibles en InlineData).
        public static TheoryData<string, string, Type> Colonnes => new()
        {
            { "Bandes", "Statut", typeof(StatutBande) },
            { "Stocks", "Type", typeof(TypeStock) },
            { "Stocks", "UniteMesure", typeof(UniteMesure) },
            { "SuivisJournaliers", "TypeSoin", typeof(TypeSoin) },
            { "Clients", "TypeClient", typeof(TypeClient) },
            { "Fournisseurs", "TypeFournisseur", typeof(TypeFournisseur) },
            { "Ventes", "TypeVente", typeof(TypeVente) },
            { "Ventes", "UniteMesure", typeof(UniteMesureVente) },
            { "Ventes", "ModePaiement", typeof(ModePaiement) },
            { "Achats", "TypeAchat", typeof(TypeAchat) },
            { "Achats", "UniteMesure", typeof(UniteMesureAchat) },
            { "Utilisateurs", "Role", typeof(RoleUtilisateur) },
            { "Alertes", "TypeAlerte", typeof(TypeAlerte) },
            { "Alertes", "Niveau", typeof(NiveauAlerte) },
            { "Parametres", "Categorie", typeof(CategorieParametre) },
        };

        [Theory]
        [MemberData(nameof(Colonnes))]
        public async Task EnumCSharp_IdentiqueALaColonneSql(string table, string colonne, Type enumCSharp)
        {
            var valeursSql = await ValeursEnumSqlAsync(table, colonne);
            var valeursCSharp = Enum.GetNames(enumCSharp).OrderBy(v => v).ToArray();

            // En cas d'échec, xUnit affiche les deux listes : on voit tout de suite la valeur en trop
            Assert.Equal(valeursCSharp, valeursSql.OrderBy(v => v).ToArray());
        }

        [Fact]
        public async Task ToutesLesColonnesEnumSontCouvertes()
        {
            // Si une colonne ENUM est ajoutée en base, ce test rappelle de l'ajouter à la liste ci-dessus
            await using var connexion = new MySqlConnection(BaseDeTest.ChaineDeConnexion());
            await connexion.OpenAsync();
            await using var commande = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = @base AND DATA_TYPE = 'enum'",
                connexion);
            commande.Parameters.AddWithValue("@base", BaseDeTest.NomBase);

            Assert.Equal(Colonnes.Count, Convert.ToInt32(await commande.ExecuteScalarAsync()));
        }

        /// <summary>
        /// Lit la définition d'une colonne ENUM, ex. "enum('EnPreparation','EnCours','Cloturee')",
        /// et en extrait les valeurs.
        /// </summary>
        private static async Task<string[]> ValeursEnumSqlAsync(string table, string colonne)
        {
            await using var connexion = new MySqlConnection(BaseDeTest.ChaineDeConnexion());
            await connexion.OpenAsync();
            // Sous Windows, MySQL stocke les noms de tables en minuscules : comparaison sans casse
            await using var commande = new MySqlCommand(
                "SELECT COLUMN_TYPE FROM information_schema.COLUMNS " +
                "WHERE TABLE_SCHEMA = @base AND LOWER(TABLE_NAME) = LOWER(@table) AND COLUMN_NAME = @colonne",
                connexion);
            commande.Parameters.AddWithValue("@base", BaseDeTest.NomBase);
            commande.Parameters.AddWithValue("@table", table);
            commande.Parameters.AddWithValue("@colonne", colonne);

            var definition = (string?)await commande.ExecuteScalarAsync()
                ?? throw new InvalidOperationException($"Colonne {table}.{colonne} introuvable");

            // "enum('A','B')" -> "'A','B'" -> ["A", "B"]
            var interieur = definition[(definition.IndexOf('(') + 1)..definition.LastIndexOf(')')];
            return interieur.Split(',').Select(v => v.Trim().Trim('\'')).ToArray();
        }
    }
}
