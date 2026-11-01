using Application.Features.Fournisseurs.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Fournisseur (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien FournisseurProfile (AutoMapper).
    /// </summary>
    public static class FournisseurMappings
    {
        /// <summary>
        /// Fournisseur (entité) -> FournisseurResponseDto.
        /// - TypeFournisseur : enum converti en texte ("Aliment")
        /// - TypeFournisseurLibelle : texte lisible calculé par l'entité
        /// - NombreAchats / MontantTotalAchats : restent à 0 ici ; un handler peut les remplir
        /// </summary>
        public static FournisseurResponseDto ToResponseDto(this Fournisseur fournisseur)
        {
            return new FournisseurResponseDto
            {
                Id = fournisseur.Id,
                Nom = fournisseur.Nom,
                TypeFournisseur = fournisseur.TypeFournisseur.ToString(),
                TypeFournisseurLibelle = fournisseur.TypeFournisseurLibelle,
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

        /// <summary>Liste de fournisseurs -> liste de DTOs.</summary>
        public static List<FournisseurResponseDto> ToResponseDtos(this IEnumerable<Fournisseur> fournisseurs)
        {
            return fournisseurs.Select(fournisseur => fournisseur.ToResponseDto()).ToList();
        }
    }
}
