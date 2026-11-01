using Application.Features.SuivisJournaliers.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions SuiviJournalier (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien SuiviProfile (AutoMapper).
    /// </summary>
    public static class SuiviMappings
    {
        /// <summary>
        /// SuiviJournalier (entité) -> SuiviResponseDto.
        /// - TypeSoin : enum nullable -> texte, ou null s'il n'y a pas eu de soin ce jour-là
        /// - BandeCode, StockConsommeNom, UniteMesureStock, CreateurNom, DateArriveeBande :
        ///   non renseignés ici ; certains handlers les complètent ensuite
        /// - PeseeEffectuee, TypeSoinLibelle, TemperatureAvis, AgeEnJours : calculés par le DTO
        /// </summary>
        public static SuiviResponseDto ToResponseDto(this SuiviJournalier suivi)
        {
            return new SuiviResponseDto
            {
                Id = suivi.Id,
                BandeId = suivi.BandeId,
                Jour = suivi.Jour,
                DateSuivi = suivi.DateSuivi,
                NbMortalites = suivi.NbMortalites,
                CauseMortalite = suivi.CauseMortalite,
                PoidsMoyen = suivi.PoidsMoyen,
                NbSujetsPeses = suivi.NbSujetsPeses,
                TypeSoin = suivi.TypeSoin.HasValue ? suivi.TypeSoin.Value.ToString() : null,
                ProduitUtilise = suivi.ProduitUtilise,
                Dosage = suivi.Dosage,
                DureeTraitement = suivi.DureeTraitement,
                CoutSoin = suivi.CoutSoin,
                StockConsommeId = suivi.StockConsommeId,
                QuantiteConsommee = suivi.QuantiteConsommee,
                Temperature = suivi.Temperature,
                Observations = suivi.Observations,
                CreatedAt = suivi.CreatedAt,
                CreatedBy = suivi.CreatedBy
            };
        }

        /// <summary>Liste de suivis -> liste de DTOs.</summary>
        public static List<SuiviResponseDto> ToResponseDtos(this IEnumerable<SuiviJournalier> suivis)
        {
            return suivis.Select(suivi => suivi.ToResponseDto()).ToList();
        }
    }
}
