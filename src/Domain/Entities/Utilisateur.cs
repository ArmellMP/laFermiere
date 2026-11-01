using Domain.Enums;

namespace Domain.Entities
{
    public class Utilisateur
    {
        public int Id { get; set; }
        
        // ==== INFORMATIONS DE BASE ====
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        // ==== RÔLE ET STATUT ====
        public RoleUtilisateur Role { get; set; }
        public bool EstActif { get; set; } = true;
        
        // ==== ACTIVITÉ ====
        public DateTime? DerniereConnexion { get; set; }
        
        // ==== AUDIT ====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // ==== PROPRIÉTÉS CALCULÉES ====
        public string RoleLibelle => GetRoleLibelle(Role);
        
        private string GetRoleLibelle(RoleUtilisateur role)
        {
            return role switch
            {
                RoleUtilisateur.Admin => "Administrateur",
                RoleUtilisateur.Fermier => "Fermier",
                RoleUtilisateur.Veterinaire => "Vétérinaire",
                RoleUtilisateur.Comptable => "Comptable",
                _ => "Inconnu"
            };
        }
        
        // ==== MÉTHODES UTILES ====
        public bool EstAdmin => Role == RoleUtilisateur.Admin;
        public bool EstFermier => Role == RoleUtilisateur.Fermier;
        public bool EstVeterinaire => Role == RoleUtilisateur.Veterinaire;
        public bool EstComptable => Role == RoleUtilisateur.Comptable;
        
        public bool APermission(params RoleUtilisateur[] roles)
        {
            return roles.Contains(Role);
        }
    }
}