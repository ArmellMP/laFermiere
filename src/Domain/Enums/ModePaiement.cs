namespace Domain.Enums
{
    /// <summary>
    /// Mode de paiement pour les ventes
    /// </summary>
    public enum ModePaiement
    {
        Especes = 1,
        PaiementMobile = 2,
        VirementBancaire = 3,
        Cheque = 4,
        Credit = 5
    }
}