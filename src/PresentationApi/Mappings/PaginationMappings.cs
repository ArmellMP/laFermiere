using PresentationApi.Models.Requests;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Découpe une liste déjà filtrée et triée en une page.
    ///
    /// La découpe se fait ici, après la requête : le format de réponse est fixé dès maintenant
    /// pour le front-end. Si les volumes l'exigent un jour, la pagination pourra descendre
    /// dans les requêtes SQL (LIMIT / OFFSET) sans changer ce format.
    /// </summary>
    public static class PaginationMappings
    {
        public static PageResponse<T> ToPage<T>(this IReadOnlyCollection<T> elements, PaginationRequest pagination)
        {
            // En long : une page très grande (?page=2000000000) ne doit pas déborder d'un int
            var aSauter = (long)(pagination.Page - 1) * pagination.Taille;

            return new PageResponse<T>
            {
                Items = aSauter >= elements.Count
                    ? new List<T>()
                    : elements.Skip((int)aSauter).Take(pagination.Taille).ToList(),
                Page = pagination.Page,
                Taille = pagination.Taille,
                Total = elements.Count
            };
        }
    }
}
