namespace Application.Common.Exceptions
{
    /// <summary>
    /// Exception lancée lorsqu'une règle métier est violée.
    /// Différente des exceptions de validation.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
        
        public BusinessRuleException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}