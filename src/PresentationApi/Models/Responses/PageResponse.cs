namespace PresentationApi.Models.Responses
{
    /// <summary>
    /// Une page d'une liste : les éléments de la page et de quoi construire la navigation.
    /// {
    ///   "items": [ ... ],
    ///   "page": 2, "taille": 20,
    ///   "total": 137,        // nombre d'éléments correspondant aux filtres, toutes pages confondues
    ///   "totalPages": 7
    /// }
    /// Une page au-delà de la dernière renvoie "items": [] avec les mêmes totaux.
    /// </summary>
    public class PageResponse<T>
    {
        public List<T> Items { get; init; } = new();
        public int Page { get; init; }
        public int Taille { get; init; }
        public int Total { get; init; }
        public int TotalPages => Total == 0 ? 0 : (int)Math.Ceiling(Total / (double)Taille);
    }
}
