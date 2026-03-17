namespace Application.Features.Bandes.Dtos
{
    /// <summary>
    /// DTO pour les statistiques des bandes.
    /// Utilisé pour le dashboard.
    /// </summary>
    public class BandeStatsDto
    {
        public int TotalBandes { get; set; }
        public int BandesEnPreparation { get; set; }
        public int BandesEnCours { get; set; }
        public int BandesCloturees { get; set; }
        public int BandesArchivees { get; set; }
        
        // Calcul des pourcentages
        public decimal PourcentageEnPreparation => TotalBandes > 0 
            ? (BandesEnPreparation / (decimal)TotalBandes) * 100 
            : 0;
            
        public decimal PourcentageEnCours => TotalBandes > 0 
            ? (BandesEnCours / (decimal)TotalBandes) * 100 
            : 0;
            
        public decimal PourcentageCloturees => TotalBandes > 0 
            ? (BandesCloturees / (decimal)TotalBandes) * 100 
            : 0;
            
        public decimal PourcentageArchivees => TotalBandes > 0 
            ? (BandesArchivees / (decimal)TotalBandes) * 100 
            : 0;
    }
}