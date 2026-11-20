namespace Application.Common.Security
{
    /// <summary>
    /// L'utilisateur qui fait la requête, lu dans son jeton de connexion (et non dans le corps
    /// de la requête, que le client pourrait falsifier). Sert à enregistrer l'auteur d'une saisie.
    /// </summary>
    public interface IUtilisateurCourant
    {
        /// <summary>Identifiant de l'utilisateur connecté ; null hors requête authentifiée (tâche de fond).</summary>
        int? Id { get; }
    }
}
