using Application.Features.Alertes.Commands.CreateAlerte;
using Application.Features.Alertes.Commands.UpdateAlerte;
using Application.Features.Alertes.Dtos;
using PresentationApi.Models.Requests.Alertes;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les alertes.
    /// Remplace l'ancien Profiles/AlerteProfile (AutoMapper).
    /// </summary>
    public static class AlerteApiMappings
    {
        /// <summary>CreateAlerteRequest -> CreateAlerteCommand (les données vont dans CreateAlerteDto).</summary>
        public static CreateAlerteCommand ToCommand(this CreateAlerteRequest request)
        {
            return new CreateAlerteCommand
            {
                CreateAlerteDto = new CreateAlerteDto
                {
                    TypeAlerte = request.TypeAlerte,
                    EntiteId = request.EntiteId,
                    Niveau = request.Niveau,
                    Message = request.Message
                }
            };
        }

        /// <summary>UpdateAlerteRequest -> UpdateAlerteCommand (marquer lue / non lue, et par qui).</summary>
        public static UpdateAlerteCommand ToCommand(this UpdateAlerteRequest request)
        {
            return new UpdateAlerteCommand
            {
                UpdateAlerteDto = new UpdateAlerteDto
                {
                    Id = request.Id,
                    EstLue = request.EstLue,
                    UtilisateurId = request.UtilisateurId
                }
            };
        }

        /// <summary>AlerteResponseDto -> AlerteResponse (JSON renvoyé au client HTTP).</summary>
        public static AlerteResponse ToResponse(this AlerteResponseDto dto)
        {
            return new AlerteResponse
            {
                Id = dto.Id,
                TypeAlerte = dto.TypeAlerte,
                TypeAlerteLibelle = dto.TypeAlerteLibelle,
                EntiteId = dto.EntiteId,
                Niveau = dto.Niveau,
                NiveauLibelle = dto.NiveauLibelle,
                Message = dto.Message,
                EstLue = dto.EstLue,
                DateLecture = dto.DateLecture,
                DateAlerte = dto.DateAlerte,
                UtilisateurId = dto.UtilisateurId,
                UtilisateurNom = dto.UtilisateurNom
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<AlerteResponse> ToResponses(this IEnumerable<AlerteResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
