namespace Application.Common.Exceptions
{
    /// <summary>
    /// Exception lancée lorsqu'une ressource n'est pas trouvée.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
        
        public NotFoundException(string name, object key)
            : base($"L'entité \"{name}\" ({key}) n'a pas été trouvée.")
        {
        }
    }
}