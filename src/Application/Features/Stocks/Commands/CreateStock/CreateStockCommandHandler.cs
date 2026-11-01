using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Stocks.Interfaces;
using Application.Features.Stocks.Dtos;
using Application.Common.Exceptions; 
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Stocks.Commands.CreateStock
{
    /// <summary>
    /// Handler pour la création d'un stock
    /// Contient la logique métier associée à la commande.
    /// </summary>
    public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, StockResponseDto>
    {
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<CreateStockCommandHandler> _logger;

        public CreateStockCommandHandler(
            IStockRepository stockRepository,
            ILogger<CreateStockCommandHandler> logger)
        {
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<StockResponseDto> Handle(CreateStockCommand request, CancellationToken cancellationToken)
        {
            var createDto = request.CreateStockDto;

            _logger.LogInformation("Création d'un nouveau stock : {Nom}", createDto.Nom);

            // 1. Validation des règles métier
            await ValidateBusinessRulesAsync(createDto, cancellationToken);

            // 2. Conversion DTO -> Entity (mapping manuel, voir StockMappings)
            var stock = createDto.ToEntity();

            // 3. Conversion sécurisée des enums
            if (!Enum.TryParse<TypeStock>(createDto.Type, true, out var typeStock))
            {
                throw new BusinessRuleException($"Type de stock invalide : {createDto.Type}");
            }

            if (!Enum.TryParse<UniteMesure>(createDto.UniteMesure, true, out var uniteMesure))
            {
                throw new BusinessRuleException($"Unité de mesure invalide : {createDto.UniteMesure}");
            }

            stock.Type = typeStock;
            stock.UniteMesure = uniteMesure;

            // 4. Initialisation des quantités
            stock.QuantiteActuelle = createDto.QuantiteInitiale;

            // 5. Dates système
            stock.CreatedAt = DateTime.UtcNow;
            stock.UpdatedAt = DateTime.UtcNow;

            // 6. Sauvegarde en base
            var createdStock = await _stockRepository.AddAsync(stock, cancellationToken);

            _logger.LogInformation("Stock créé avec succès : {Id}", createdStock.Id);

            // 7. Conversion Entity -> DTO
            return createdStock.ToResponseDto();
        }

        /// <summary>
        /// Vérifie les règles métier avant la création du stock.
        /// </summary>
        private async Task ValidateBusinessRulesAsync(CreateStockDto dto, CancellationToken cancellationToken)
        {
            // Règle 1 : Le seuil d'alerte ne peut pas dépasser la quantité initiale
            if (dto.SeuilAlerte.HasValue && dto.SeuilAlerte > dto.QuantiteInitiale)
            {
                throw new BusinessRuleException(
                    "Le seuil d'alerte ne peut pas être supérieur à la quantité initiale.");
            }

            // Règle 2 : Vérification du type
            if (!Enum.TryParse<TypeStock>(dto.Type, true, out var typeStock))
            {
                throw new BusinessRuleException($"Type de stock invalide : {dto.Type}");
            }

            // Règle 3 : Médicament et vaccin nécessitent une date de péremption
            if ((typeStock == TypeStock.Medicament || typeStock == TypeStock.Vaccin)
                && !dto.DatePeremption.HasValue)
            {
                throw new BusinessRuleException(
                    "La date de péremption est obligatoire pour les médicaments et vaccins.");
            }

            await Task.CompletedTask;
        }
    }
}
