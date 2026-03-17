using Application.Features.Stocks.Dtos;
using Application.Features.Stocks.Commands.CreateStock;
using Application.Features.Stocks.Commands.UpdateStock;
using Application.Features.Stocks.Commands.ConsommerStock;
using Application.Common.Exceptions; 
using Domain.Entities;
using Domain.Enums;
using AutoMapper; 

namespace Application.Common.Mappings
{
    public class StockProfile : Profile
    {
        public StockProfile()
        {
            // ============================================
            // 1. MAPPING ENTITÉ → DTO DE RÉPONSE
            // ============================================
            
            CreateMap<Stock, StockResponseDto>()
                .ForMember(dest => dest.Type, 
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.UniteMesure, 
                    opt => opt.MapFrom(src => src.UniteMesure.ToString()))
                .ForMember(dest => dest.TypeLibelle, opt => opt.Ignore()) // Calculé dans DTO
                .ForMember(dest => dest.UniteMesureLibelle, opt => opt.Ignore()) // Calculé dans DTO
                .ForMember(dest => dest.BandeCode, opt => opt.Ignore()) // Rempli par JOIN
                .ForMember(dest => dest.FournisseurNom, opt => opt.Ignore()); // Rempli par JOIN

            // ============================================
            // 2. MAPPING POUR LA CRÉATION (CreateStockDto → Stock)
            // ============================================
            
            CreateMap<CreateStockDto, Stock>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore()) // Converti dans Handler
                .ForMember(dest => dest.UniteMesure, opt => opt.Ignore()) // Converti dans Handler
                .ForMember(dest => dest.QuantiteActuelle, opt => opt.Ignore()) // = Initiale
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // ============================================
            // 3. MAPPING POUR LA MISE À JOUR (UpdateStockDto → Stock)
            // ============================================
            
            CreateMap<UpdateStockDto, Stock>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.QuantiteActuelle, opt => opt.MapFrom(src => src.QuantiteActuelle))
                .ForMember(dest => dest.SeuilAlerte, opt => opt.MapFrom(src => src.SeuilAlerte))
                .ForMember(dest => dest.DatePeremption, opt => opt.MapFrom(src => src.DatePeremption))
                .ForMember(dest => dest.PrixUnitaire, opt => opt.MapFrom(src => src.PrixUnitaire))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Défini dans handler
                
                // Propriétés non modifiables
                .ForMember(dest => dest.BandeId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Nom, opt => opt.Ignore())
                .ForMember(dest => dest.QuantiteInitiale, opt => opt.Ignore())
                .ForMember(dest => dest.UniteMesure, opt => opt.Ignore())
                .ForMember(dest => dest.FournisseurId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // ============================================
            // 4. MAPPING POUR LA COMMANDE CONSOMMER
            // ============================================
            
            CreateMap<ConsommerStockCommand, Stock>()
                .ForMember(dest => dest.QuantiteActuelle, 
                    opt => opt.MapFrom(src => src.QuantiteConsommee)) // Attention : c'est la quantité à soustraire
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}