namespace PresentationApi.Models.Responses
{
    /// <summary>
    /// Résultat de POST /api/v1/Alertes/generer : nombre d'alertes créées, par type.
    /// </summary>
    public class GenerationAlertesResponse
    {
        public int StockFaible { get; set; }
        public int Peremption { get; set; }
        public int BandeACloturer { get; set; }
        public int Total { get; set; }
    }
}
