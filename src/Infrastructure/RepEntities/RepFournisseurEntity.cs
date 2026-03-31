using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepFournisseurEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string TypeFournisseur { get; set; } = string.Empty;  // string (SQL ENUM)
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? ContactPersonne { get; set; }
        public string? Notes { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS ====
        public int? NombreAchats { get; set; }
        public decimal? MontantTotalAchats { get; set; }

        /// <summary>
        /// Convertit RepFournisseurEntity en entité Domain Fournisseur
        /// </summary>
        public Fournisseur ToDomain()
        {
            // ✅ CORRECTION : Utiliser le nom complet de l'enum
            TypeFournisseur typeFournisseur = Domain.Enums.TypeFournisseur.Autre;
            
            if (!string.IsNullOrEmpty(TypeFournisseur))
            {
                if (Enum.TryParse<TypeFournisseur>(TypeFournisseur, true, out var parsed))
                {
                    typeFournisseur = parsed;
                }
            }

            return new Fournisseur
            {
                Id = this.Id,
                Nom = this.Nom,
                TypeFournisseur = typeFournisseur,
                Telephone = this.Telephone,
                Email = this.Email,
                Adresse = this.Adresse,
                ContactPersonne = this.ContactPersonne,
                Notes = this.Notes,
                EstActif = this.EstActif,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Fournisseur en RepFournisseurEntity
        /// </summary>
        public static RepFournisseurEntity FromDomain(Fournisseur fournisseur)
        {
            return new RepFournisseurEntity
            {
                Id = fournisseur.Id,
                Nom = fournisseur.Nom,
                TypeFournisseur = fournisseur.TypeFournisseur.ToString(),
                Telephone = fournisseur.Telephone,
                Email = fournisseur.Email,
                Adresse = fournisseur.Adresse,
                ContactPersonne = fournisseur.ContactPersonne,
                Notes = fournisseur.Notes,
                EstActif = fournisseur.EstActif,
                CreatedAt = fournisseur.CreatedAt,
                UpdatedAt = fournisseur.UpdatedAt
            };
        }

        public static implicit operator Fournisseur?(RepFournisseurEntity? rep)
        {
            return rep?.ToDomain();
        }

        public static implicit operator RepFournisseurEntity?(Fournisseur? fournisseur)
        {
            return fournisseur == null ? null : FromDomain(fournisseur);
        }
    }
}