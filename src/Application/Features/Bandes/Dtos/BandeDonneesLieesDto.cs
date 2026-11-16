namespace Application.Features.Bandes.Dtos
{
    /// <summary>
    /// Nombre d'enregistrements rattachés à une bande, table par table.
    /// Sert à refuser la suppression d'une bande qui n'est pas vide.
    /// </summary>
    public class BandeDonneesLieesDto
    {
        public int NbSuivis { get; set; }
        public int NbStocks { get; set; }
        public int NbVentes { get; set; }
        public int NbAchats { get; set; }

        public bool EstVide => NbSuivis == 0 && NbStocks == 0 && NbVentes == 0 && NbAchats == 0;
    }
}
