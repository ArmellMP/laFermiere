namespace Application.Common.Exceptions
{
    /// <summary>
    /// Exception lancée quand la donnée envoyée existe déjà (e-mail déjà utilisé, clé de
    /// paramètre déjà prise, suivi déjà saisi pour cette date...).
    /// L'ExceptionMiddleware la traduit en 409 Conflict.
    ///
    /// Elle hérite de BusinessRuleException : un code qui intercepte les règles métier
    /// en général continue d'intercepter aussi les doublons.
    /// </summary>
    public class ConflictException : BusinessRuleException
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
