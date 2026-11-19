namespace Domain.Entities
{
    /// <summary>
    /// Refresh token : permet d'obtenir un nouveau jeton d'accès (JWT) sans ressaisir
    /// le mot de passe. Seule son empreinte SHA-256 (TokenHash) est enregistrée.
    ///
    /// Une connexion ouvre une "famille" (FamilleId = la session). Chaque renouvellement
    /// marque le token utilisé (UtiliseLe) et en crée un nouveau dans la même famille.
    /// Toutes les dates sont en UTC.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string FamilleId { get; set; } = string.Empty;
        public DateTime ExpireLe { get; set; }
        public DateTime? UtiliseLe { get; set; }
        public DateTime? RevoqueLe { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool EstExpire(DateTime maintenantUtc) => ExpireLe <= maintenantUtc;
    }
}
