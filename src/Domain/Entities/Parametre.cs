// Domain/Entities/Parametre.cs
using Domain.Enums;

namespace Domain.Entities
{
    public class Parametre
    {
        public int Id { get; set; }
        
        // ==== IDENTIFICATION ====
        public string Cle { get; set; } = string.Empty;
        public string? Valeur { get; set; }
        public string? Description { get; set; }
        
        // ==== CLASSIFICATION ====
        public CategorieParametre Categorie { get; set; }
        
        // ==== CONTRÔLE ====
        public bool EstModifiable { get; set; } = true;
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // ==== PROPRIÉTÉ CALCULÉE ====
        public string CategorieLibelle => GetCategorieLibelle(Categorie);
        
        private string GetCategorieLibelle(CategorieParametre categorie)
        {
            return categorie switch
            {
                CategorieParametre.Elevage => "Élevage",
                CategorieParametre.Stock => "Stock",
                CategorieParametre.Finance => "Finance",
                CategorieParametre.Systeme => "Système",
                CategorieParametre.General => "Général",
                _ => "Inconnu"
            };
        }
    }
}