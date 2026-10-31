using Application.Features.Bandes.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions de l'entité Bande (couche Domain) vers ses DTOs (couche Application).
    ///
    /// Ce sont des "méthodes d'extension" : le mot-clé "this" devant le premier
    /// paramètre permet d'écrire bande.ToResponseDto() comme si la méthode
    /// appartenait à la classe Bande, sans modifier cette classe.
    ///
    /// Remplace l'ancien BandeProfile (AutoMapper) : chaque propriété copiée est
    /// écrite explicitement, rien n'est caché ni deviné à l'exécution.
    /// </summary>
    public static class BandeMappings
    {
        /// <summary>
        /// Bande (entité) -> BandeResponseDto.
        /// Les propriétés calculées du DTO (StatutLibelle, TauxMortalite, JoursEcoules...)
        /// ne sont pas copiées : le DTO les calcule lui-même à partir des valeurs ci-dessous.
        /// IncludeDetails reste à false : c'est le handler qui l'active si besoin.
        /// </summary>
        public static BandeResponseDto ToResponseDto(this Bande bande)
        {
            return new BandeResponseDto
            {
                Id = bande.Id,
                Code = bande.Code,
                DateArrivee = bande.DateArrivee,
                NbPoussinsInit = bande.NbPoussinsInit,
                NbPoussinsActuel = bande.NbPoussinsActuel,
                Statut = bande.Statut,
                DateCloture = bande.DateCloture,
                Observations = bande.Observations,
                SuperficieCalculee = bande.SuperficieCalculee,
                CreatedAt = bande.CreatedAt,
                UpdatedAt = bande.UpdatedAt
            };
        }

        /// <summary>
        /// Liste de bandes -> liste de DTOs (applique ToResponseDto à chaque élément).
        /// </summary>
        public static List<BandeResponseDto> ToResponseDtos(this IEnumerable<Bande> bandes)
        {
            return bandes.Select(bande => bande.ToResponseDto()).ToList();
        }
    }
}
