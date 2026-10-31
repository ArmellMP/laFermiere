using Application.Features.SuivisJournaliers.Commands.CreateSuivi;
using Application.Features.SuivisJournaliers.Commands.UpdateSuivi;
using Application.Features.SuivisJournaliers.Dtos;
using PresentationApi.Models.Requests.Suivis;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les suivis journaliers.
    /// Remplace l'ancien Profiles/SuiviProfile (AutoMapper).
    /// </summary>
    public static class SuiviApiMappings
    {
        /// <summary>CreateSuiviRequest -> CreateSuiviCommand (les données vont dans CreateSuiviDto).</summary>
        public static CreateSuiviCommand ToCommand(this CreateSuiviRequest request)
        {
            return new CreateSuiviCommand
            {
                CreateSuiviDto = new CreateSuiviDto
                {
                    BandeId = request.BandeId,
                    Jour = request.Jour,
                    DateSuivi = request.DateSuivi,
                    NbMortalites = request.NbMortalites,
                    CauseMortalite = request.CauseMortalite,
                    PoidsMoyen = request.PoidsMoyen,
                    NbSujetsPeses = request.NbSujetsPeses,
                    TypeSoin = request.TypeSoin,
                    ProduitUtilise = request.ProduitUtilise,
                    Dosage = request.Dosage,
                    DureeTraitement = request.DureeTraitement,
                    CoutSoin = request.CoutSoin,
                    StockConsommeId = request.StockConsommeId,
                    QuantiteConsommee = request.QuantiteConsommee,
                    Temperature = request.Temperature,
                    Observations = request.Observations
                }
            };
        }

        /// <summary>
        /// UpdateSuiviRequest -> UpdateSuiviCommand.
        /// Les champs laissés à null signifient "ne pas modifier".
        /// </summary>
        public static UpdateSuiviCommand ToCommand(this UpdateSuiviRequest request)
        {
            return new UpdateSuiviCommand
            {
                UpdateSuiviDto = new UpdateSuiviDto
                {
                    Id = request.Id,
                    NbMortalites = request.NbMortalites,
                    CauseMortalite = request.CauseMortalite,
                    PoidsMoyen = request.PoidsMoyen,
                    NbSujetsPeses = request.NbSujetsPeses,
                    TypeSoin = request.TypeSoin,
                    ProduitUtilise = request.ProduitUtilise,
                    Dosage = request.Dosage,
                    DureeTraitement = request.DureeTraitement,
                    CoutSoin = request.CoutSoin,
                    StockConsommeId = request.StockConsommeId,
                    QuantiteConsommee = request.QuantiteConsommee,
                    Temperature = request.Temperature,
                    Observations = request.Observations
                }
            };
        }

        /// <summary>SuiviResponseDto -> SuiviResponse (JSON renvoyé au client HTTP).</summary>
        public static SuiviResponse ToResponse(this SuiviResponseDto dto)
        {
            return new SuiviResponse
            {
                Id = dto.Id,
                BandeId = dto.BandeId,
                BandeCode = dto.BandeCode,
                Jour = dto.Jour,
                DateSuivi = dto.DateSuivi,
                NbMortalites = dto.NbMortalites,
                CauseMortalite = dto.CauseMortalite,
                PoidsMoyen = dto.PoidsMoyen,
                NbSujetsPeses = dto.NbSujetsPeses,
                PeseeEffectuee = dto.PeseeEffectuee,
                TypeSoin = dto.TypeSoin,
                TypeSoinLibelle = dto.TypeSoinLibelle,
                ProduitUtilise = dto.ProduitUtilise,
                Dosage = dto.Dosage,
                DureeTraitement = dto.DureeTraitement,
                CoutSoin = dto.CoutSoin,
                StockConsommeId = dto.StockConsommeId,
                StockConsommeNom = dto.StockConsommeNom,
                QuantiteConsommee = dto.QuantiteConsommee,
                UniteMesureStock = dto.UniteMesureStock,
                Temperature = dto.Temperature,
                TemperatureAvis = dto.TemperatureAvis,
                Observations = dto.Observations,
                CreatedAt = dto.CreatedAt,
                CreatedBy = dto.CreatedBy,
                CreateurNom = dto.CreateurNom,
                AgeEnJours = dto.AgeEnJours
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<SuiviResponse> ToResponses(this IEnumerable<SuiviResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
