using Application.Features.Achats.Dtos;
using Application.Features.Achats.Validators;
using Application.Features.Ventes.Dtos;
using Application.Features.Ventes.Validators;
using FluentValidation.TestHelper;

namespace LaFermiere.UnitTests.Validators
{
    /// <summary>
    /// Tests des règles de validation des ventes et des achats.
    /// Décisions métier testées (DECISIONS.md, 2026-11-01) :
    ///   n°7 : dates de vente et d'achat jamais dans le futur
    ///   n°8 : vente payée => date de paiement obligatoire, pas dans le futur
    /// + réception d'un achat jamais avant la date d'achat.
    /// </summary>
    public class VenteEtAchatValidatorsTests
    {
        private readonly CreateVenteDtoValidator _venteValidator = new();
        private readonly CreateAchatDtoValidator _achatValidator = new();

        private static CreateVenteDto VenteValide() => new()
        {
            BandeId = 1,
            ClientId = 1,
            DateVente = DateTime.Today.AddDays(-2),
            TypeVente = "PouletsVivants",
            Quantite = 10,
            UniteMesure = "Unite",
            PrixUnitaire = 2500,
            ModePaiement = "Especes",
            EstPaye = true,
            DatePaiement = DateTime.Today.AddDays(-2)
        };

        private static CreateAchatDto AchatValide() => new()
        {
            FournisseurId = 1,
            BandeId = 1,
            DateAchat = DateTime.Today.AddDays(-5),
            TypeAchat = "Aliment",
            Quantite = 50,
            UniteMesure = "Kg",
            PrixUnitaire = 300,
            DateReception = DateTime.Today.AddDays(-4)
        };

        [Fact]
        public void Vente_VenteValide_AucuneErreur()
        {
            _venteValidator.TestValidate(VenteValide()).ShouldNotHaveAnyValidationErrors();
        }

        // Décision n°7
        [Fact]
        public void Vente_DateDansLeFutur_Refusee()
        {
            var vente = VenteValide();
            vente.DateVente = DateTime.Today.AddDays(1);

            _venteValidator.TestValidate(vente).ShouldHaveValidationErrorFor(x => x.DateVente);
        }

        // Décision n°8 : règle CONDITIONNELLE (When(x => x.EstPaye, ...)).
        // On teste les deux branches : payée et non payée.
        [Fact]
        public void Vente_PayeeSansDatePaiement_Refusee()
        {
            var vente = VenteValide();
            vente.EstPaye = true;
            vente.DatePaiement = null;

            _venteValidator.TestValidate(vente).ShouldHaveValidationErrorFor(x => x.DatePaiement);
        }

        [Fact]
        public void Vente_NonPayeeSansDatePaiement_Acceptee()
        {
            var vente = VenteValide();
            vente.EstPaye = false;          // vente à crédit, par exemple
            vente.DatePaiement = null;

            _venteValidator.TestValidate(vente).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Vente_DatePaiementDansLeFutur_Refusee()
        {
            var vente = VenteValide();
            vente.DatePaiement = DateTime.Today.AddDays(1);

            _venteValidator.TestValidate(vente).ShouldHaveValidationErrorFor(x => x.DatePaiement);
        }

        [Fact]
        public void Achat_AchatValide_AucuneErreur()
        {
            _achatValidator.TestValidate(AchatValide()).ShouldNotHaveAnyValidationErrors();
        }

        // Décision n°7
        [Fact]
        public void Achat_DateDansLeFutur_Refusee()
        {
            var achat = AchatValide();
            achat.DateAchat = DateTime.Today.AddDays(1);
            achat.DateReception = null;

            _achatValidator.TestValidate(achat).ShouldHaveValidationErrorFor(x => x.DateAchat);
        }

        // Règle entre deux champs : impossible avec les DataAnnotations, d'où l'intérêt de FluentValidation
        [Theory]
        [InlineData(-1, false)]  // reçu la veille de l'achat : impossible
        [InlineData(0, true)]    // reçu le jour même
        [InlineData(3, true)]    // reçu 3 jours après
        public void Achat_DateReceptionParRapportALAchat(int joursApresAchat, bool estValide)
        {
            var achat = AchatValide();
            achat.DateReception = achat.DateAchat.AddDays(joursApresAchat);

            var resultat = _achatValidator.TestValidate(achat);

            if (estValide)
                resultat.ShouldNotHaveValidationErrorFor(x => x.DateReception);
            else
                resultat.ShouldHaveValidationErrorFor(x => x.DateReception);
        }
    }
}
