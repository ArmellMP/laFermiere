using Domain.Enums;

namespace Domain.Entities
{
    public class Achat
    {
        public int Id { get; set; }
        
        // ==== CLÉS ÉTRANGÈRES ====
        public int FournisseurId { get; set; }
        public int BandeId { get; set; }
        public int? CreatedBy { get; set; }
        
        // ==== INFORMATIONS D'ACHAT ====
        public DateTime DateAchat { get; set; }
        public TypeAchat TypeAchat { get; set; }
        public string? Description { get; set; }
        public decimal Quantite { get; set; }
        public UniteMesureAchat UniteMesure { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        
        // ==== FACTURATION ====
        public string? FactureNumero { get; set; }
        public DateTime? DateReception { get; set; }
        
        // ==== NOTES ====
        public string? Observations { get; set; }
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        
        // ==== PROPRIÉTÉS CALCULÉES ====
        public bool EstRecu => DateReception.HasValue;
        public int JoursEntreAchatEtReception => DateReception.HasValue 
            ? (DateReception.Value - DateAchat).Days 
            : 0;
        
        // ==== PROPRIÉTÉS DE NAVIGATION (pour plus tard) ====
        // public virtual Fournisseur? Fournisseur { get; set; }
        // public virtual Bande? Bande { get; set; }
        // public virtual Utilisateur? Createur { get; set; }
    }
}