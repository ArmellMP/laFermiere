using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Représente un fournisseur (poussins, aliments, médicaments, etc.)
    /// </summary>
    public class Fournisseur
    {
        public int Id { get; set; }
        
        // ==== INFORMATIONS DE BASE ====
        public string Nom { get; set; } = string.Empty;
        public TypeFournisseur TypeFournisseur { get; set; }
        
        // ==== CONTACT ====
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        
        // ==== ADRESSE ====
        public string? Adresse { get; set; }
        
        // ==== PERSONNE DE CONTACT ====
        public string? ContactPersonne { get; set; }
        
        // ==== NOTES ====
        public string? Notes { get; set; }
        
        // ==== STATUT ====
        public bool EstActif { get; set; } = true;
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // ==== PROPRIÉTÉ CALCULÉE ====
        public string TypeFournisseurLibelle => GetTypeFournisseurLibelle(TypeFournisseur);
        
        private string GetTypeFournisseurLibelle(TypeFournisseur type)
        {
            return type switch
            {
                TypeFournisseur.Poussiniere => "Poussinière",
                TypeFournisseur.Aliment => "Aliment",
                TypeFournisseur.Medicament => "Médicament",
                TypeFournisseur.Materiel => "Matériel",
                TypeFournisseur.Services => "Services",
                TypeFournisseur.Transport => "Transport",
                TypeFournisseur.Autre => "Autre",
                _ => "Inconnu"
            };
        }
        
        // ==== PROPRIÉTÉS DE NAVIGATION (pour plus tard) ====
        // public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();
    }
}