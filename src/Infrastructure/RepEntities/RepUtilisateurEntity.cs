using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepUtilisateurEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;  // string (SQL ENUM)
        public bool EstActif { get; set; }
        public DateTime? DerniereConnexion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Convertit RepUtilisateurEntity en entité Domain Utilisateur
        /// </summary>
        public Utilisateur ToDomain()
        {
            // Conversion sécurisée du rôle
            RoleUtilisateur role = RoleUtilisateur.Fermier;
            if (!string.IsNullOrEmpty(Role))
            {
                if (Enum.TryParse<RoleUtilisateur>(Role, true, out var parsed))
                {
                    role = parsed;
                }
            }

            return new Utilisateur
            {
                Id = this.Id,
                Nom = this.Nom,
                Email = this.Email,
                PasswordHash = this.PasswordHash,
                Role = role,
                EstActif = this.EstActif,
                DerniereConnexion = this.DerniereConnexion,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Utilisateur en RepUtilisateurEntity
        /// </summary>
        public static RepUtilisateurEntity FromDomain(Utilisateur utilisateur)
        {
            return new RepUtilisateurEntity
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                PasswordHash = utilisateur.PasswordHash,
                Role = utilisateur.Role.ToString(),
                EstActif = utilisateur.EstActif,
                DerniereConnexion = utilisateur.DerniereConnexion,
                CreatedAt = utilisateur.CreatedAt,
                UpdatedAt = utilisateur.UpdatedAt
            };
        }

        public static implicit operator Utilisateur?(RepUtilisateurEntity? rep)
        {
            return rep?.ToDomain();
        }

        public static implicit operator RepUtilisateurEntity?(Utilisateur? utilisateur)
        {
            return utilisateur == null ? null : FromDomain(utilisateur);
        }
    }
}