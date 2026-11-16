using Application.Features.SuivisJournaliers.Dtos;
using Application.Features.SuivisJournaliers.Validators;
using FluentValidation.TestHelper;

namespace LaFermiere.UnitTests.Validators
{
    /// <summary>
    /// Tests des règles de validation du suivi journalier.
    /// Décisions métier testées (DECISIONS.md, 2026-11-01) :
    ///   n°7  : date de suivi jamais dans le futur
    ///   n°10 : poids moyen en kg (0,01 à 10) ; jusqu'à 10 000 sujets pesés
    /// + les règles conditionnelles : une pesée exige le nombre de sujets pesés,
    ///   un soin exige le produit, un stock consommé exige la quantité.
    /// </summary>
    public class SuiviJournalierValidatorsTests
    {
        private readonly CreateSuiviDtoValidator _validator = new();

        // Suivi minimal valide : aucune pesée, aucun soin, aucun stock consommé
        private static CreateSuiviDto SuiviValide() => new()
        {
            BandeId = 1,
            Jour = 5,
            DateSuivi = DateTime.Today.AddDays(-1),
            NbMortalites = 0
        };

        [Fact]
        public void SuiviMinimal_AucuneErreur()
        {
            _validator.TestValidate(SuiviValide()).ShouldNotHaveAnyValidationErrors();
        }

        // Décision n°7
        [Fact]
        public void DateSuiviDansLeFutur_Refusee()
        {
            var suivi = SuiviValide();
            suivi.DateSuivi = DateTime.Today.AddDays(1);

            _validator.TestValidate(suivi).ShouldHaveValidationErrorFor(x => x.DateSuivi);
        }

        // Décision n°10 : poids en kg
        [Theory]
        [InlineData(0.05, true)]
        [InlineData(2.5, true)]
        [InlineData(10, true)]      // borne haute acceptée
        [InlineData(10.01, false)]  // au-dessus de 10 kg : refusé
        [InlineData(0, false)]
        [InlineData(1500, false)]   // 1500 = des grammes saisis par erreur : refusé
        public void PoidsMoyen_EnKilogrammes(double poids, bool estValide)
        {
            var suivi = SuiviValide();
            suivi.PoidsMoyen = (decimal)poids;
            suivi.NbSujetsPeses = 10;

            var resultat = _validator.TestValidate(suivi);

            if (estValide)
                resultat.ShouldNotHaveValidationErrorFor(x => x.PoidsMoyen);
            else
                resultat.ShouldHaveValidationErrorFor(x => x.PoidsMoyen);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(10000, true)]
        [InlineData(10001, false)]
        [InlineData(0, false)]
        public void NbSujetsPeses_Limites(int nbSujets, bool estValide)
        {
            var suivi = SuiviValide();
            suivi.PoidsMoyen = 2.5m;
            suivi.NbSujetsPeses = nbSujets;

            var resultat = _validator.TestValidate(suivi);

            if (estValide)
                resultat.ShouldNotHaveValidationErrorFor(x => x.NbSujetsPeses);
            else
                resultat.ShouldHaveValidationErrorFor(x => x.NbSujetsPeses);
        }

        // Règles conditionnelles : chacune testée dans ses deux branches
        [Fact]
        public void Pesee_SansNombreDeSujets_Refusee()
        {
            var suivi = SuiviValide();
            suivi.PoidsMoyen = 2.5m;
            suivi.NbSujetsPeses = null;

            _validator.TestValidate(suivi).ShouldHaveValidationErrorFor(x => x.NbSujetsPeses);
        }

        [Fact]
        public void SansPesee_NombreDeSujetsNonExige()
        {
            var suivi = SuiviValide();   // PoidsMoyen null : la règle ne s'applique pas

            _validator.TestValidate(suivi).ShouldNotHaveValidationErrorFor(x => x.NbSujetsPeses);
        }

        [Fact]
        public void Soin_SansProduit_Refuse()
        {
            var suivi = SuiviValide();
            suivi.TypeSoin = "Vaccination";
            suivi.ProduitUtilise = null;

            _validator.TestValidate(suivi).ShouldHaveValidationErrorFor(x => x.ProduitUtilise);
        }

        [Fact]
        public void Soin_AvecProduit_Accepte()
        {
            var suivi = SuiviValide();
            suivi.TypeSoin = "Vaccination";
            suivi.ProduitUtilise = "Gumboro";

            _validator.TestValidate(suivi).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void StockConsomme_SansQuantite_Refuse()
        {
            var suivi = SuiviValide();
            suivi.StockConsommeId = 3;
            suivi.QuantiteConsommee = null;

            _validator.TestValidate(suivi).ShouldHaveValidationErrorFor(x => x.QuantiteConsommee);
        }
    }
}
