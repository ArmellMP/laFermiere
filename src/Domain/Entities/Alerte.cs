using Domain.Enums;

namespace Domain.Entities
{
    public class Alerte
    {
        public int Id { get; set; }
        
        // ==== IDENTIFICATION ====
        public TypeAlerte TypeAlerte { get; set; }
        public int EntiteId { get; set; }  // ID de la bande, stock, etc.
        public NiveauAlerte Niveau { get; set; }
        public string Message { get; set; } = string.Empty;
        
        // ==== STATUT ====
        public bool EstLue { get; set; }
        public DateTime? DateLecture { get; set; }
        
        // ==== AUDIT ====
        public DateTime DateAlerte { get; set; }
        public int? UtilisateurId { get; set; }  // Utilisateur qui a lu l'alerte
        
        // ==== PROPRIÉTÉS CALCULÉES ====
        public string TypeAlerteLibelle => GetTypeAlerteLibelle(TypeAlerte);
        public string NiveauLibelle => GetNiveauLibelle(Niveau);
        
        private string GetTypeAlerteLibelle(TypeAlerte type)
        {
            return type switch
            {
                TypeAlerte.StockFaible => "Stock faible",
                TypeAlerte.Peremption => "Péremption",
                TypeAlerte.MortaliteElevee => "Mortalité élevée",
                TypeAlerte.Temperature => "Température anormale",
                TypeAlerte.PeseeManquante => "Pesée manquante",
                TypeAlerte.BandeACloturer => "Bande à clôturer",
                _ => "Inconnu"
            };
        }
        
        private string GetNiveauLibelle(NiveauAlerte niveau)
        {
            return niveau switch
            {
                NiveauAlerte.Info => "Information",
                NiveauAlerte.Avertissement => "Avertissement",
                NiveauAlerte.Critique => "Critique",
                _ => "Inconnu"
            };
        }
        
        // ==== MÉTHODES UTILES ====
        public void MarquerCommeLue(int utilisateurId)
        {
            EstLue = true;
            DateLecture = DateTime.UtcNow;
            UtilisateurId = utilisateurId;
        }
        
        public bool EstNonLue => !EstLue;
    }
}