using Application.Features.Stocks.Commands.ConsommerStock;
using Application.Features.Stocks.Commands.CreateStock;
using Application.Features.Stocks.Commands.UpdateStock;
using Application.Features.Stocks.Dtos;
using PresentationApi.Models.Requests.Stocks;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les stocks.
    /// Remplace l'ancien Profiles/StockProfile (AutoMapper).
    /// </summary>
    public static class StockApiMappings
    {
        // ============================================
        // 1. REQUÊTES HTTP -> COMMANDS
        // ============================================

        /// <summary>CreateStockRequest -> CreateStockCommand (les données vont dans CreateStockDto).</summary>
        public static CreateStockCommand ToCommand(this CreateStockRequest request)
        {
            return new CreateStockCommand
            {
                CreateStockDto = new CreateStockDto
                {
                    BandeId = request.BandeId,
                    Type = request.Type,
                    Nom = request.Nom,
                    QuantiteInitiale = request.QuantiteInitiale,
                    UniteMesure = request.UniteMesure,
                    SeuilAlerte = request.SeuilAlerte,
                    DatePeremption = request.DatePeremption,
                    PrixUnitaire = request.PrixUnitaire,
                    FournisseurId = request.FournisseurId
                }
            };
        }

        /// <summary>UpdateStockRequest -> UpdateStockCommand (les données vont dans UpdateStockDto).</summary>
        public static UpdateStockCommand ToCommand(this UpdateStockRequest request)
        {
            return new UpdateStockCommand
            {
                UpdateStockDto = new UpdateStockDto
                {
                    Id = request.Id,
                    QuantiteActuelle = request.QuantiteActuelle,
                    SeuilAlerte = request.SeuilAlerte,
                    DatePeremption = request.DatePeremption,
                    PrixUnitaire = request.PrixUnitaire
                }
            };
        }

        /// <summary>ConsommerStockRequest -> ConsommerStockCommand (mêmes champs).</summary>
        public static ConsommerStockCommand ToCommand(this ConsommerStockRequest request)
        {
            return new ConsommerStockCommand
            {
                StockId = request.StockId,
                QuantiteConsommee = request.QuantiteConsommee,
                SuiviJournalierId = request.SuiviJournalierId
            };
        }

        // ============================================
        // 2. DTO (Application) -> RÉPONSE HTTP
        // ============================================

        /// <summary>
        /// StockResponseDto -> StockResponse : copie champ par champ.
        /// Les valeurs calculées du DTO (EstEnAlerte, ValeurStock...) sont figées ici
        /// dans la réponse JSON.
        /// </summary>
        public static StockResponse ToResponse(this StockResponseDto dto)
        {
            return new StockResponse
            {
                Id = dto.Id,
                BandeId = dto.BandeId,
                BandeCode = dto.BandeCode,
                Type = dto.Type,
                TypeLibelle = dto.TypeLibelle,
                Nom = dto.Nom,
                QuantiteInitiale = dto.QuantiteInitiale,
                QuantiteActuelle = dto.QuantiteActuelle,
                UniteMesure = dto.UniteMesure,
                UniteMesureLibelle = dto.UniteMesureLibelle,
                SeuilAlerte = dto.SeuilAlerte,
                EstEnAlerte = dto.EstEnAlerte,
                DatePeremption = dto.DatePeremption,
                EstPerime = dto.EstPerime,
                JoursAvantPeremption = dto.JoursAvantPeremption,
                PrixUnitaire = dto.PrixUnitaire,
                ValeurStock = dto.ValeurStock,
                FournisseurId = dto.FournisseurId,
                FournisseurNom = dto.FournisseurNom,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<StockResponse> ToResponses(this IEnumerable<StockResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
