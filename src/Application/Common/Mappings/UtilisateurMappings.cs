using Application.Features.Utilisateurs.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Utilisateur (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien UtilisateurProfile (AutoMapper).
    /// </summary>
    public static class UtilisateurMappings
    {
        /// <summary>
        /// Utilisateur (entité) -> UtilisateurResponseDto.
        /// SÉCURITÉ : PasswordHash n'est volontairement PAS copié. Le hash du mot de passe
        /// ne doit jamais quitter la couche Infrastructure / Domain.
        /// - Role : enum converti en texte ("Admin") ; RoleLibelle calculé par l'entité
        /// - EstAdmin, EstFermier... : calculés par le DTO à partir de Role
        /// </summary>
        public static UtilisateurResponseDto ToResponseDto(this Utilisateur utilisateur)
        {
            return new UtilisateurResponseDto
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                Role = utilisateur.Role.ToString(),
                RoleLibelle = utilisateur.RoleLibelle,
                EstActif = utilisateur.EstActif,
                DerniereConnexion = utilisateur.DerniereConnexion,
                CreatedAt = utilisateur.CreatedAt,
                UpdatedAt = utilisateur.UpdatedAt
            };
        }

        /// <summary>Liste d'utilisateurs -> liste de DTOs.</summary>
        public static List<UtilisateurResponseDto> ToResponseDtos(this IEnumerable<Utilisateur> utilisateurs)
        {
            return utilisateurs.Select(utilisateur => utilisateur.ToResponseDto()).ToList();
        }
    }
}
