using Application.Features.Stocks.Commands.CreateStock;
using Application.Features.Stocks.Dtos;
using Application.Features.Stocks.Validators;
using FluentValidation.TestHelper;

namespace LaFermiere.UnitTests.Validators
{
    /// <summary>
    /// Tests des règles de validation des stocks.
    /// Décisions métier testées (DECISIONS.md, 2026-11-01) :
    ///   n°4 : date de péremption future exigée à la CRÉATION seulement
    ///   n°5 : nom de stock libre (100 caractères max)
    ///   n°6 : seuil d'alerte <= quantité initiale
    /// + la règle existante "unité cohérente avec le type" (validateur de la Command).
    /// </summary>
    public class StockValidatorsTests
    {
        private readonly CreateStockDtoValidator _createValidator = new();
        private readonly UpdateStockDtoValidator _updateValidator = new();
        private readonly CreateStockCommandValidator _commandValidator = new();

        private static CreateStockDto StockValide() => new()
        {
            BandeId = 1,
            Type = "Aliment",
            Nom = "Aliment démarrage",
            QuantiteInitiale = 100,
            UniteMesure = "Kg",
            SeuilAlerte = 10,
            DatePeremption = DateTime.Today.AddMonths(6),
            PrixUnitaire = 300,
            FournisseurId = 1
        };

        [Fact]
        public void Creation_StockValide_AucuneErreur()
        {
            _createValidator.TestValidate(StockValide()).ShouldNotHaveAnyValidationErrors();
        }

        // Décision n°5 : la regex d'origine refusait parenthèses, barres obliques, apostrophes...
        [Theory]
        [InlineData("Vaccin (Gumboro)")]
        [InlineData("Aliment 1/2")]
        [InlineData("Médicament d'urgence")]
        [InlineData("Lot n°3 + vitamines")]
        public void Creation_NomAvecCaracteresSpeciaux_Accepte(string nom)
        {
            var stock = StockValide();
            stock.Nom = nom;

            _createValidator.TestValidate(stock).ShouldNotHaveValidationErrorFor(x => x.Nom);
        }

        [Fact]
        public void Creation_NomTropLong_Refuse()
        {
            var stock = StockValide();
            stock.Nom = new string('a', 101);   // 101 caractères : un de trop

            _createValidator.TestValidate(stock).ShouldHaveValidationErrorFor(x => x.Nom);
        }

        [Fact]
        public void Creation_NomVide_Refuse()
        {
            var stock = StockValide();
            stock.Nom = "";

            _createValidator.TestValidate(stock).ShouldHaveValidationErrorFor(x => x.Nom);
        }

        // Décision n°4, côté création
        [Fact]
        public void Creation_DatePeremptionPassee_Refusee()
        {
            var stock = StockValide();
            stock.DatePeremption = DateTime.Today.AddDays(-1);

            _createValidator.TestValidate(stock).ShouldHaveValidationErrorFor(x => x.DatePeremption);
        }

        [Fact]
        public void Creation_DatePeremptionAPlusDe5Ans_Refusee()
        {
            var stock = StockValide();
            stock.DatePeremption = DateTime.Today.AddYears(6);

            _createValidator.TestValidate(stock).ShouldHaveValidationErrorFor(x => x.DatePeremption);
        }

        [Fact]
        public void Creation_SansDatePeremption_Acceptee()
        {
            var stock = StockValide();
            stock.DatePeremption = null;   // matériel, par exemple : pas de péremption

            _createValidator.TestValidate(stock).ShouldNotHaveValidationErrorFor(x => x.DatePeremption);
        }

        // Décision n°4, côté modification : il faut pouvoir corriger un stock déjà périmé
        [Fact]
        public void Modification_DatePeremptionPassee_Acceptee()
        {
            var modification = new UpdateStockDto
            {
                Id = 1,
                QuantiteActuelle = 50,
                DatePeremption = DateTime.Today.AddDays(-30)
            };

            _updateValidator.TestValidate(modification).ShouldNotHaveValidationErrorFor(x => x.DatePeremption);
        }

        // Décision n°6
        [Theory]
        [InlineData(0, true)]
        [InlineData(100, true)]     // seuil = quantité : accepté (borne)
        [InlineData(100.01, false)] // un centième de trop : refusé
        [InlineData(-1, false)]
        public void Creation_SeuilAlerte_NeDepassePasLaQuantite(double seuil, bool estValide)
        {
            // InlineData n'accepte pas le type decimal (limite du langage C# pour les attributs) :
            // on passe un double et on le convertit.
            var stock = StockValide();
            stock.SeuilAlerte = (decimal)seuil;

            var resultat = _createValidator.TestValidate(stock);

            if (estValide)
                resultat.ShouldNotHaveValidationErrorFor(x => x.SeuilAlerte);
            else
                resultat.ShouldHaveValidationErrorFor(x => x.SeuilAlerte);
        }

        // Règle de la Command : l'unité doit correspondre au type de stock.
        // Cette règle porte sur l'objet entier (RuleFor(x => x)), pas sur un champ :
        // on vérifie donc le message plutôt que le nom du champ.
        [Theory]
        [InlineData("Aliment", "Kg", true)]
        [InlineData("Aliment", "Sac", true)]
        [InlineData("Aliment", "Dose", false)]
        [InlineData("Vaccin", "Dose", true)]
        [InlineData("Materiel", "Dose", false)]
        [InlineData("Autre", "Dose", true)]       // "Autre" : pas de restriction
        public void Commande_UniteCoherenteAvecLeType(string type, string unite, bool estValide)
        {
            var stock = StockValide();
            stock.Type = type;
            stock.UniteMesure = unite;
            var commande = new CreateStockCommand { CreateStockDto = stock };

            var resultat = _commandValidator.TestValidate(commande);

            // Assert.Equal compare deux valeurs : ici, "y a-t-il une erreur d'unité ?" et "en attend-on une ?"
            var erreurUnite = resultat.Errors.Any(e => e.ErrorMessage.Contains("unité de mesure n'est pas cohérente"));
            Assert.Equal(!estValide, erreurUnite);
        }
    }
}
