using Application.Features.Achats.Commands.CreateAchat;
using Application.Features.Achats.Commands.UpdateAchat;
using Application.Features.Achats.Dtos;
using PresentationApi.Models.Requests.Achats;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les achats.
    /// Remplace l'ancien Profiles/AchatProfile (AutoMapper).
    /// </summary>
    public static class AchatApiMappings
    {
        /// <summary>CreateAchatRequest -> CreateAchatCommand (les données vont dans CreateAchatDto).</summary>
        public static CreateAchatCommand ToCommand(this CreateAchatRequest request)
        {
            return new CreateAchatCommand
            {
                CreateAchatDto = new CreateAchatDto
                {
                    FournisseurId = request.FournisseurId,
                    BandeId = request.BandeId,
                    DateAchat = request.DateAchat,
                    TypeAchat = request.TypeAchat,
                    Description = request.Description,
                    Quantite = request.Quantite,
                    UniteMesure = request.UniteMesure,
                    PrixUnitaire = request.PrixUnitaire,
                    FactureNumero = request.FactureNumero,
                    DateReception = request.DateReception,
                    Observations = request.Observations
                }
            };
        }

        /// <summary>
        /// UpdateAchatRequest -> UpdateAchatCommand.
        /// Les champs laissés à null signifient "ne pas modifier".
        /// </summary>
        public static UpdateAchatCommand ToCommand(this UpdateAchatRequest request)
        {
            return new UpdateAchatCommand
            {
                UpdateAchatDto = new UpdateAchatDto
                {
                    Id = request.Id,
                    DateAchat = request.DateAchat,
                    TypeAchat = request.TypeAchat,
                    Description = request.Description,
                    Quantite = request.Quantite,
                    UniteMesure = request.UniteMesure,
                    PrixUnitaire = request.PrixUnitaire,
                    FactureNumero = request.FactureNumero,
                    DateReception = request.DateReception,
                    Observations = request.Observations
                }
            };
        }

        /// <summary>AchatResponseDto -> AchatResponse (JSON renvoyé au client HTTP).</summary>
        public static AchatResponse ToResponse(this AchatResponseDto dto)
        {
            return new AchatResponse
            {
                Id = dto.Id,
                FournisseurId = dto.FournisseurId,
                FournisseurNom = dto.FournisseurNom,
                BandeId = dto.BandeId,
                BandeCode = dto.BandeCode,
                DateAchat = dto.DateAchat,
                TypeAchat = dto.TypeAchat,
                TypeAchatLibelle = dto.TypeAchatLibelle,
                Description = dto.Description,
                Quantite = dto.Quantite,
                UniteMesure = dto.UniteMesure,
                UniteMesureLibelle = dto.UniteMesureLibelle,
                PrixUnitaire = dto.PrixUnitaire,
                MontantTotal = dto.MontantTotal,
                FactureNumero = dto.FactureNumero,
                DateReception = dto.DateReception,
                EstRecu = dto.EstRecu,
                JoursEntreAchatEtReception = dto.JoursEntreAchatEtReception,
                Observations = dto.Observations,
                CreatedAt = dto.CreatedAt,
                CreatedBy = dto.CreatedBy,
                CreateurNom = dto.CreateurNom
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<AchatResponse> ToResponses(this IEnumerable<AchatResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
