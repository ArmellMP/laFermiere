// PresentationApi/Profiles/StockProfile.cs
using AutoMapper;
using PresentationApi.Models.Requests.Stocks;
using PresentationApi.Models.Responses;
using Application.Features.Stocks.Commands.CreateStock;
using Application.Features.Stocks.Commands.UpdateStock;
using Application.Features.Stocks.Commands.ConsommerStock;
using Application.Features.Stocks.Dtos;
using Domain.Enums;

namespace PresentationApi.Profiles
{
    public class StockProfile : Profile
    {
        public StockProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            // CreateStockRequest → CreateStockCommand
            CreateMap<CreateStockRequest, CreateStockCommand>()
                .ForMember(dest => dest.CreateStockDto, 
                    opt => opt.MapFrom(src => src));
            
            // CreateStockRequest → CreateStockDto
            CreateMap<CreateStockRequest, CreateStockDto>();
            
            // UpdateStockRequest → UpdateStockCommand
            CreateMap<UpdateStockRequest, UpdateStockCommand>()
                .ForMember(dest => dest.UpdateStockDto, 
                    opt => opt.MapFrom(src => src));
            
            // UpdateStockRequest → UpdateStockDto
            CreateMap<UpdateStockRequest, UpdateStockDto>();
            
            // ConsommerStockRequest → ConsommerStockCommand
            CreateMap<ConsommerStockRequest, ConsommerStockCommand>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            // StockResponseDto (Application) → StockResponse (API)
            CreateMap<StockResponseDto, StockResponse>();
        }
    }
}