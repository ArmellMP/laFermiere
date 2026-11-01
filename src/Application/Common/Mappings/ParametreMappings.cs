using Application.Features.Parametres.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Parametre (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien ParametreProfile (AutoMapper).
    /// </summary>
    public static class ParametreMappings
    {
        /// <summary>
        /// Parametre (entité) -> ParametreResponseDto.
        /// - Categorie : enum converti en texte ("Elevage")
        /// - CategorieLibelle : texte lisible calculé par l'entité ("Élevage")
        /// </summary>
        public static ParametreResponseDto ToResponseDto(this Parametre parametre)
        {
            return new ParametreResponseDto
            {
                Id = parametre.Id,
                Cle = parametre.Cle,
                Valeur = parametre.Valeur,
                Description = parametre.Description,
                Categorie = parametre.Categorie.ToString(),
                CategorieLibelle = parametre.CategorieLibelle,
                EstModifiable = parametre.EstModifiable,
                CreatedAt = parametre.CreatedAt,
                UpdatedAt = parametre.UpdatedAt
            };
        }

        /// <summary>Liste de paramètres -> liste de DTOs.</summary>
        public static List<ParametreResponseDto> ToResponseDtos(this IEnumerable<Parametre> parametres)
        {
            return parametres.Select(parametre => parametre.ToResponseDto()).ToList();
        }
    }
}
