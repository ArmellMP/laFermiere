using Domain.Enums;

namespace Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }
        
        // ==== INFORMATIONS DE BASE ====
        public string Nom { get; set; } = string.Empty;
        public TypeClient TypeClient { get; set; }
        
        // ==== CONTACT ====
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        
        // ==== ADRESSE ====
        public string? Adresse { get; set; }
        
        // ==== NOTES ====
        public string? Notes { get; set; }
        
        // ==== STATUT ====
        public bool EstActif { get; set; } = true;
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // ==== PROPRIÉTÉ CALCULÉE ====
        public string TypeClientLibelle => GetTypeClientLibelle(TypeClient);
        
        private string GetTypeClientLibelle(TypeClient type)
        {
            return type switch
            {
                TypeClient.Particulier => "Particulier",
                TypeClient.Restaurant => "Restaurant",
                TypeClient.Marche => "Marché",
                TypeClient.Grossiste => "Grossiste",
                TypeClient.Transformateur => "Transformateur",
                TypeClient.Autre => "Autre",
                _ => "Inconnu"
            };
        }
        
        // ==== PROPRIÉTÉS DE NAVIGATION (pour plus tard) ====
        // public virtual ICollection<Vente> Ventes { get; set; } = new List<Vente>();
    }
}