// PresentationAPI/Profiles/BandeProfile.cs
using AutoMapper;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Commands.CreateBande;
using Application.Features.Bandes.Commands.UpdateBande;
using Application.Features.Bandes.Commands.CloturerBande;
using PresentationAPI.Models.Requests.Bandes;
using PresentationAPI.Models.Responses;
using Domain.Enums;

namespace PresentationAPI.Profiles
{
    public class BandeProfile : Profile
    {
        public BandeProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS / DTOS
            // ============================================
            
            // CreateBandeRequest → CreateBandeCommand
            CreateMap<CreateBandeRequest, CreateBandeCommand>()
                .ForMember(dest => dest.CreateBandeDto, 
                    opt => opt.MapFrom(src => src));
            
            // CreateBandeRequest → CreateBandeDto
            CreateMap<CreateBandeRequest, CreateBandeDto>();
            
            // UpdateBandeRequest → UpdateBandeCommand
            CreateMap<UpdateBandeRequest, UpdateBandeCommand>()
                .ForMember(dest => dest.UpdateBandeDto, 
                    opt => opt.MapFrom(src => src));
            
            // UpdateBandeRequest → UpdateBandeDto
            CreateMap<UpdateBandeRequest, UpdateBandeDto>()
                .ForMember(dest => dest.Statut, 
                    opt => opt.MapFrom(src => Enum.Parse<StatutBande>(src.Statut)));
            
            // CloturerBandeRequest → CloturerBandeCommand
            CreateMap<CloturerBandeRequest, CloturerBandeCommand>();
            
            // ============================================
            // 2. RESPONSES (Application → API)
            // ============================================
            
            // BandeResponseDto (Application) → BandeResponse (API)
            CreateMap<BandeResponseDto, BandeResponse>()
                .ForMember(dest => dest.Statut, 
                    opt => opt.MapFrom(src => src.Statut.ToString()))
                .ForMember(dest => dest.StatutLibelle,
                    opt => opt.MapFrom(src => src.StatutLibelle))
                .ForMember(dest => dest.TauxMortalite,
                    opt => opt.MapFrom(src => Math.Round(src.TauxMortalite, 2)))
                .ForMember(dest => dest.TotalMortalites,
                    opt => opt.MapFrom(src => src.IncludeDetails ? src.TotalMortalites : null))
                .ForMember(dest => dest.JoursEcoules,
                    opt => opt.MapFrom(src => src.IncludeDetails ? src.JoursEcoules : null))
                .ForMember(dest => dest.JoursRestants,
                    opt => opt.MapFrom(src => src.IncludeDetails ? src.JoursRestants : null))
                .ForMember(dest => dest.PourcentageAvancement,
                    opt => opt.MapFrom(src => src.IncludeDetails ? src.PourcentageAvancement : null));
        }
    }
}