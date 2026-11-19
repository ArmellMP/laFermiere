namespace Application.Features.Alertes.Dtos
{
    /// <summary>
    /// Nombre d'alertes créées par une génération automatique, par type.
    /// Une situation déjà signalée par une alerte non lue n'est pas recomptée.
    /// </summary>
    public class GenerationAlertesResultDto
    {
        public int StockFaible { get; set; }
        public int Peremption { get; set; }
        public int BandeACloturer { get; set; }

        public int Total => StockFaible + Peremption + BandeACloturer;
    }
}
