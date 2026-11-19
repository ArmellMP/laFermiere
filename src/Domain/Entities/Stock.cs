using Domain.Enums;
namespace Domain.Entities
{
    public class Stock
    {
        public int Id { get; set; }

        // ==== INFORMATIONS LIÉES (lecture seule) ====
        // Lues par jointure SQL dans le repository, pour l'affichage uniquement.
        // Jamais écrites en base : la référence reste l'identifiant (BandeId...).
        public string? BandeCode { get; set; }
        public string? FournisseurNom { get; set; }
        public int BandeId { get; set; }
        public TypeStock Type { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal QuantiteInitiale { get; set; }
        public decimal QuantiteActuelle { get; set; }
        public UniteMesure UniteMesure { get; set; }
        public decimal? SeuilAlerte { get; set; }
        public DateTime? DatePeremption { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public int? FournisseurId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        //public virtual Bande? Bande { get; set; }
        //public virtual Fournisseur? Fournisseur { get; set; }
        
        // Propriétés calculées
        public bool EstEnAlerte => QuantiteActuelle <= (SeuilAlerte ?? 0);
        public decimal ValeurStock => QuantiteActuelle * (PrixUnitaire ?? 0);
    }
}