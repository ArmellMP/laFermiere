using Domain.Enums;

namespace Domain.Entities
{
    public class Vente
    {
        public int Id { get; set; }
        
        // ==== CLÉS ÉTRANGÈRES ====
        public int BandeId { get; set; }
        public int ClientId { get; set; }
        public int? CreatedBy { get; set; }
        
        // ==== INFORMATIONS DE VENTE ====
        public DateTime DateVente { get; set; }
        public TypeVente TypeVente { get; set; }
        public decimal Quantite { get; set; }
        public UniteMesureVente UniteMesure { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        
        // ==== PAIEMENT ====
        public ModePaiement ModePaiement { get; set; }
        public bool EstPaye { get; set; }
        public DateTime? DatePaiement { get; set; }
        
        // ==== NOTES ====
        public string? Observations { get; set; }
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        
        // ==== PROPRIÉTÉS CALCULÉES ====
        public bool EstPayeEnRetard => !EstPaye && DatePaiement.HasValue && DatePaiement > DateTime.Now;
        
        // ==== PROPRIÉTÉS DE NAVIGATION (pour plus tard) ====
        // public virtual Bande? Bande { get; set; }
        // public virtual Client? Client { get; set; }
        // public virtual Utilisateur? Createur { get; set; }
    }
}