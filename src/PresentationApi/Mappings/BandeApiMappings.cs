using Application.Features.Bandes.Commands.CloturerBande;
using Application.Features.Bandes.Commands.CreateBande;
using Application.Features.Bandes.Commands.UpdateBande;
using Application.Features.Bandes.Dtos;
using Domain.Enums;
using PresentationApi.Models.Requests.Bandes;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions entre le format HTTP de l'API (Requests / Responses)
    /// et les objets de la couche Application (Commands / DTOs) pour les bandes.
    ///
    /// Sens des flux :
    ///   Requête HTTP  --ToCommand()-->  Command  --(MediatR)-->  Handler
    ///   Handler       --> BandeResponseDto --ToResponse()-->  Réponse HTTP (JSON)
    ///
    /// Remplace l'ancien Profiles/BandeProfile (AutoMapper).
    /// </summary>
    public static class BandeApiMappings
    {
        // ============================================
        // 1. REQUÊTES HTTP -> COMMANDS
        // ============================================

        /// <summary>CreateBandeRequest -> CreateBandeCommand (les données vont dans CreateBandeDto).</summary>
        public static CreateBandeCommand ToCommand(this CreateBandeRequest request)
        {
            return new CreateBandeCommand
            {
                CreateBandeDto = new CreateBandeDto
                {
                    DateArrivee = request.DateArrivee,
                    NbPoussinsInit = request.NbPoussinsInit,
                    Observations = request.Observations,
                    Code = request.Code
                }
            };
        }

        /// <summary>
        /// UpdateBandeRequest -> UpdateBandeCommand.
        /// Le statut arrive en texte ("EnCours") et devient l'enum StatutBande.
        /// </summary>
        public static UpdateBandeCommand ToCommand(this UpdateBandeRequest request)
        {
            return new UpdateBandeCommand
            {
                UpdateBandeDto = new UpdateBandeDto
                {
                    Id = request.Id,
                    NbPoussinsActuel = request.NbPoussinsActuel,
                    Observations = request.Observations,
                    Statut = Enum.Parse<StatutBande>(request.Statut),
                    DateCloture = request.DateCloture
                }
            };
        }

        /// <summary>CloturerBandeRequest -> CloturerBandeCommand (mêmes champs).</summary>
        public static CloturerBandeCommand ToCommand(this CloturerBandeRequest request)
        {
            return new CloturerBandeCommand
            {
                Id = request.Id,
                DateCloture = request.DateCloture,
                ObservationsCloture = request.ObservationsCloture
            };
        }

        // ============================================
        // 2. DTO (Application) -> RÉPONSE HTTP
        // ============================================

        /// <summary>
        /// BandeResponseDto -> BandeResponse (ce que reçoit le client en JSON).
        /// - Statut : enum converti en texte ("EnCours")
        /// - TauxMortalite : arrondi à 2 décimales
        /// - Champs de détail : renseignés seulement si IncludeDetails, sinon null
        ///   (null => absents du JSON grâce à JsonIgnoreCondition.WhenWritingNull)
        /// </summary>
        public static BandeResponse ToResponse(this BandeResponseDto dto)
        {
            return new BandeResponse
            {
                Id = dto.Id,
                Code = dto.Code,
                DateArrivee = dto.DateArrivee,
                NbPoussinsInit = dto.NbPoussinsInit,
                NbPoussinsActuel = dto.NbPoussinsActuel,
                Statut = dto.Statut.ToString(),
                StatutLibelle = dto.StatutLibelle,
                DateCloture = dto.DateCloture,
                Observations = dto.Observations,
                SuperficieCalculee = dto.SuperficieCalculee,
                TauxMortalite = Math.Round(dto.TauxMortalite, 2),
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                TotalMortalites = dto.IncludeDetails ? dto.TotalMortalites : null,
                JoursEcoules = dto.IncludeDetails ? dto.JoursEcoules : null,
                JoursRestants = dto.IncludeDetails ? dto.JoursRestants : null,
                PourcentageAvancement = dto.IncludeDetails ? dto.PourcentageAvancement : null
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<BandeResponse> ToResponses(this IEnumerable<BandeResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
