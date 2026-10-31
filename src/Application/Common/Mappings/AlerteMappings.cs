using Application.Features.Alertes.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Alerte (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien AlerteProfile (AutoMapper).
    /// </summary>
    public static class AlerteMappings
    {
        /// <summary>
        /// Alerte (entité) -> AlerteResponseDto.
        /// - TypeAlerte, Niveau : enums convertis en texte ; libellés calculés par l'entité
        /// - UtilisateurNom : non renseigné ici (null)
        /// </summary>
        public static AlerteResponseDto ToResponseDto(this Alerte alerte)
        {
            return new AlerteResponseDto
            {
                Id = alerte.Id,
                TypeAlerte = alerte.TypeAlerte.ToString(),
                TypeAlerteLibelle = alerte.TypeAlerteLibelle,
                EntiteId = alerte.EntiteId,
                Niveau = alerte.Niveau.ToString(),
                NiveauLibelle = alerte.NiveauLibelle,
                Message = alerte.Message,
                EstLue = alerte.EstLue,
                DateLecture = alerte.DateLecture,
                DateAlerte = alerte.DateAlerte,
                UtilisateurId = alerte.UtilisateurId
            };
        }

        /// <summary>Liste d'alertes -> liste de DTOs.</summary>
        public static List<AlerteResponseDto> ToResponseDtos(this IEnumerable<Alerte> alertes)
        {
            return alertes.Select(alerte => alerte.ToResponseDto()).ToList();
        }
    }
}
