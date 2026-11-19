using PresentationApi.Mappings;
using PresentationApi.Models.Requests;

namespace LaFermiere.UnitTests.Pagination
{
    /// <summary>
    /// Découpe d'une liste en pages (PaginationMappings.ToPage) et calcul de totalPages.
    /// </summary>
    public class PaginationMappingsTests
    {
        // 45 éléments : 1, 2, ..., 45
        private static readonly List<int> Elements = Enumerable.Range(1, 45).ToList();

        private static PaginationRequest Page(int page, int taille) => new() { Page = page, Taille = taille };

        [Fact]
        public void PremierePage_LesPremiersElements()
        {
            var page = Elements.ToPage(Page(1, 20));

            Assert.Equal(Enumerable.Range(1, 20), page.Items);
            Assert.Equal(45, page.Total);
            Assert.Equal(3, page.TotalPages);   // 20 + 20 + 5
        }

        [Fact]
        public void DernierePage_IncompleteMaisCorrecte()
        {
            var page = Elements.ToPage(Page(3, 20));

            Assert.Equal(new[] { 41, 42, 43, 44, 45 }, page.Items);
            Assert.Equal(3, page.Page);
        }

        [Fact]
        public void PageAuDelaDeLaDerniere_ListeVideEtMemesTotaux()
        {
            var page = Elements.ToPage(Page(4, 20));

            Assert.Empty(page.Items);
            Assert.Equal(45, page.Total);
            Assert.Equal(3, page.TotalPages);
        }

        [Fact]
        public void NumeroDePageEnorme_PasDeDebordement()
        {
            // (2 000 000 000 - 1) x 100 dépasse la capacité d'un int
            var page = Elements.ToPage(Page(2_000_000_000, 100));

            Assert.Empty(page.Items);
        }

        [Fact]
        public void ListeVide_ZeroPage()
        {
            var page = new List<int>().ToPage(Page(1, 20));

            Assert.Empty(page.Items);
            Assert.Equal(0, page.Total);
            Assert.Equal(0, page.TotalPages);
        }
    }
}
