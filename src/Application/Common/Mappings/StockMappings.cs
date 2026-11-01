using Application.Features.Stocks.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Stock (entité) <-> DTOs de la couche Application.
    /// Remplace l'ancien StockProfile (AutoMapper).
    /// </summary>
    public static class StockMappings
    {
        /// <summary>
        /// Stock (entité) -> StockResponseDto.
        /// - Type et UniteMesure : enums convertis en texte ("Aliment", "Kg")
        /// - BandeCode et FournisseurNom : non renseignés ici (null), ils viendraient d'une jointure
        /// - TypeLibelle, EstEnAlerte, ValeurStock... : calculés par le DTO lui-même
        /// </summary>
        public static StockResponseDto ToResponseDto(this Stock stock)
        {
            return new StockResponseDto
            {
                Id = stock.Id,
                BandeId = stock.BandeId,
                Type = stock.Type.ToString(),
                Nom = stock.Nom,
                QuantiteInitiale = stock.QuantiteInitiale,
                QuantiteActuelle = stock.QuantiteActuelle,
                UniteMesure = stock.UniteMesure.ToString(),
                SeuilAlerte = stock.SeuilAlerte,
                DatePeremption = stock.DatePeremption,
                PrixUnitaire = stock.PrixUnitaire,
                FournisseurId = stock.FournisseurId,
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };
        }

        /// <summary>Liste de stocks -> liste de DTOs.</summary>
        public static List<StockResponseDto> ToResponseDtos(this IEnumerable<Stock> stocks)
        {
            return stocks.Select(stock => stock.ToResponseDto()).ToList();
        }

        /// <summary>
        /// CreateStockDto -> nouvelle entité Stock (champs simples uniquement).
        /// Le handler complète ensuite : Type et UniteMesure (texte -> enum, avec contrôle),
        /// QuantiteActuelle (= QuantiteInitiale) et les dates CreatedAt / UpdatedAt.
        /// </summary>
        public static Stock ToEntity(this CreateStockDto dto)
        {
            return new Stock
            {
                BandeId = dto.BandeId,
                Nom = dto.Nom,
                QuantiteInitiale = dto.QuantiteInitiale,
                SeuilAlerte = dto.SeuilAlerte,
                DatePeremption = dto.DatePeremption,
                PrixUnitaire = dto.PrixUnitaire,
                FournisseurId = dto.FournisseurId
            };
        }
    }
}
