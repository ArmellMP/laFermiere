namespace Application.Common.Security
{
    /// <summary>
    /// Hachage et vérification des mots de passe avec BCrypt.
    /// BCrypt est volontairement lent et ajoute un sel aléatoire à chaque hash :
    /// deux utilisateurs avec le même mot de passe obtiennent des hash différents.
    /// </summary>
    public static class PasswordHasher
    {
        // Facteur de coût : 2^12 itérations. Plus il est élevé, plus le calcul est lent
        // (donc plus une attaque par force brute coûte cher). 12 est la valeur courante.
        private const int WorkFactor = 12;

        public static string HashPassword(string password)
        {
            // Le sel est généré automatiquement et stocké dans le hash lui-même
            // (format "$2a$12$<sel><hash>"), pas besoin de colonne séparée.
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                // BCrypt relit le sel et le coût dans le hash, recalcule et compare.
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Le hash stocké n'est pas au format BCrypt (ex : ancien hash SHA256).
                // On refuse la connexion au lieu de faire planter la requête.
                return false;
            }
        }
    }
}
