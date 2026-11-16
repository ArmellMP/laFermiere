using Application.Features.Bandes.Commands.CloturerBande;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace LaFermiere.UnitTests.Validators
{
    /// <summary>
    /// Tests des règles de validation des bandes.
    /// Décisions métier testées (DECISIONS.md, 2026-11-01) :
    ///   n°1 : 50 000 poussins maximum
    ///   n°2 : date d'arrivée jamais dans le futur
    ///   n°3 : date de clôture jamais dans le futur (clôture ET modification)
    ///
    /// Outil : FluentValidation.TestHelper (fourni avec FluentValidation).
    ///   validator.TestValidate(objet)                        lance la validation
    ///   .ShouldHaveValidationErrorFor(x => x.Champ)          exige une erreur sur ce champ
    ///   .ShouldNotHaveValidationErrorFor(x => x.Champ)       exige l'absence d'erreur sur ce champ
    ///   .ShouldNotHaveAnyValidationErrors()                  exige un objet entièrement valide
    ///
    /// Méthode : partir d'un objet VALIDE, changer UN seul champ, vérifier l'effet.
    /// Si le test échoue, on sait exactement quelle règle est en cause.
    /// </summary>
    public class BandeValidatorsTests
    {
        // Le validateur testé. xUnit crée une NOUVELLE instance de la classe de test
        // pour chaque test : chaque test part donc d'un état neuf.
        private readonly CreateBandeDtoValidator _createValidator = new();
        private readonly UpdateBandeDtoValidator _updateValidator = new();
        private readonly CloturerBandeCommandValidator _cloturerValidator = new();

        // Objets valides de référence : chaque test en modifie un seul champ
        private static CreateBandeDto BandeValide() => new()
        {
            DateArrivee = DateTime.Today.AddDays(-10),
            NbPoussinsInit = 500,
            Observations = null,
            Code = null
        };

        private static UpdateBandeDto ModificationValide() => new()
        {
            Id = 1,
            NbPoussinsActuel = 480,
            Statut = StatutBande.EnCours,
            DateCloture = null
        };

        [Fact]
        public void Creation_BandeValide_AucuneErreur()
        {
            var resultat = _createValidator.TestValidate(BandeValide());

            resultat.ShouldNotHaveAnyValidationErrors();
        }

        // Décision n°1 : les limites exactes (1 et 50 000) sont acceptées.
        // Tester les BORNES est essentiel : c'est là que se cachent les erreurs
        // du genre "<" écrit à la place de "<=".
        [Theory]
        [InlineData(1)]
        [InlineData(50000)]
        public void Creation_NombreDePoussinsDansLesLimites_Accepte(int nbPoussins)
        {
            var bande = BandeValide();
            bande.NbPoussinsInit = nbPoussins;

            var resultat = _createValidator.TestValidate(bande);

            resultat.ShouldNotHaveValidationErrorFor(x => x.NbPoussinsInit);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(50001)]
        public void Creation_NombreDePoussinsHorsLimites_Refuse(int nbPoussins)
        {
            var bande = BandeValide();
            bande.NbPoussinsInit = nbPoussins;

            var resultat = _createValidator.TestValidate(bande);

            resultat.ShouldHaveValidationErrorFor(x => x.NbPoussinsInit);
        }

        // Décision n°2
        [Fact]
        public void Creation_DateArriveeAujourdhui_Acceptee()
        {
            var bande = BandeValide();
            bande.DateArrivee = DateTime.Today;

            var resultat = _createValidator.TestValidate(bande);

            resultat.ShouldNotHaveValidationErrorFor(x => x.DateArrivee);
        }

        [Fact]
        public void Creation_DateArriveeDemain_Refusee()
        {
            var bande = BandeValide();
            bande.DateArrivee = DateTime.Today.AddDays(1);

            var resultat = _createValidator.TestValidate(bande);

            resultat.ShouldHaveValidationErrorFor(x => x.DateArrivee);
        }

        // Décision n°1 appliquée à la modification
        [Theory]
        [InlineData(0, true)]       // tous les poulets vendus ou morts : accepté
        [InlineData(50000, true)]
        [InlineData(50001, false)]
        [InlineData(-1, false)]
        public void Modification_NombreDePoussinsActuel_RespecteLesLimites(int nbPoussins, bool estValide)
        {
            var modification = ModificationValide();
            modification.NbPoussinsActuel = nbPoussins;

            var resultat = _updateValidator.TestValidate(modification);

            // Un seul test pour les deux cas : le paramètre estValide dit lequel on attend
            if (estValide)
                resultat.ShouldNotHaveValidationErrorFor(x => x.NbPoussinsActuel);
            else
                resultat.ShouldHaveValidationErrorFor(x => x.NbPoussinsActuel);
        }

        [Fact]
        public void Modification_StatutClotureeSansDate_Refusee()
        {
            var modification = ModificationValide();
            modification.Statut = StatutBande.Cloturee;
            modification.DateCloture = null;

            var resultat = _updateValidator.TestValidate(modification);

            resultat.ShouldHaveValidationErrorFor(x => x.DateCloture);
        }

        // Décision n°3 via la modification (PUT /Bandes/{id})
        [Fact]
        public void Modification_DateClotureDansLeFutur_Refusee()
        {
            var modification = ModificationValide();
            modification.Statut = StatutBande.Cloturee;
            modification.DateCloture = DateTime.Today.AddDays(1);

            var resultat = _updateValidator.TestValidate(modification);

            resultat.ShouldHaveValidationErrorFor(x => x.DateCloture);
        }

        [Fact]
        public void Modification_ClotureAvecDatePassee_Acceptee()
        {
            var modification = ModificationValide();
            modification.Statut = StatutBande.Cloturee;
            modification.DateCloture = DateTime.Today.AddDays(-1);

            var resultat = _updateValidator.TestValidate(modification);

            resultat.ShouldNotHaveAnyValidationErrors();
        }

        // Décision n°3 via la clôture (POST /Bandes/{id}/cloturer)
        [Fact]
        public void Cloture_DateDansLeFutur_Refusee()
        {
            var commande = new CloturerBandeCommand { Id = 1, DateCloture = DateTime.Today.AddDays(1) };

            var resultat = _cloturerValidator.TestValidate(commande);

            resultat.ShouldHaveValidationErrorFor(x => x.DateCloture);
        }

        [Fact]
        public void Cloture_SansDate_Acceptee()
        {
            // Sans date, le handler utilise la date du jour : c'est autorisé
            var commande = new CloturerBandeCommand { Id = 1, DateCloture = null };

            var resultat = _cloturerValidator.TestValidate(commande);

            resultat.ShouldNotHaveAnyValidationErrors();
        }
    }
}
