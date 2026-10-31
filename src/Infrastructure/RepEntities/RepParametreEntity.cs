using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepParametreEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public string Cle { get; set; } = string.Empty;
        public string? Valeur { get; set; }
        public string? Description { get; set; }
        public string Categorie { get; set; } = string.Empty;  // string (SQL ENUM)
        public bool EstModifiable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Convertit RepParametreEntity en entité Domain Parametre
        /// </summary>
        public Parametre ToDomain()
        {
            // Conversion sécurisée de la catégorie
            CategorieParametre categorie = CategorieParametre.General;
            if (!string.IsNullOrEmpty(Categorie))
            {
                if (Enum.TryParse<CategorieParametre>(Categorie, true, out var parsed))
                {
                    categorie = parsed;
                }
            }

            return new Parametre
            {
                Id = this.Id,
                Cle = this.Cle,
                Valeur = this.Valeur,
                Description = this.Description,
                Categorie = categorie,
                EstModifiable = this.EstModifiable,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Parametre en RepParametreEntity
        /// </summary>
        public static RepParametreEntity FromDomain(Parametre parametre)
        {
            return new RepParametreEntity
            {
                Id = parametre.Id,
                Cle = parametre.Cle,
                Valeur = parametre.Valeur,
                Description = parametre.Description,
                Categorie = parametre.Categorie.ToString(),
                EstModifiable = parametre.EstModifiable,
                CreatedAt = parametre.CreatedAt,
                UpdatedAt = parametre.UpdatedAt
            };
        }

        public static implicit operator Parametre?(RepParametreEntity? rep)
        {
            return rep?.ToDomain();
        }

        public static implicit operator RepParametreEntity?(Parametre? parametre)
        {
            return parametre == null ? null : FromDomain(parametre);
        }
    }
}