using AutoMapper;
using PresentationApi.Models.Requests.Alertes;
using PresentationApi.Models.Responses;
using Application.Features.Alertes.Commands.CreateAlerte;
using Application.Features.Alertes.Commands.UpdateAlerte;
using Application.Features.Alertes.Dtos;

namespace PresentationApi.Profiles
{
    public class AlerteProfile : Profile
    {
        public AlerteProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            CreateMap<CreateAlerteRequest, CreateAlerteCommand>()
                .ForMember(dest => dest.CreateAlerteDto, opt => opt.MapFrom(src => src));
            
            CreateMap<CreateAlerteRequest, CreateAlerteDto>();
            
            CreateMap<UpdateAlerteRequest, UpdateAlerteCommand>()
                .ForMember(dest => dest.UpdateAlerteDto, opt => opt.MapFrom(src => src));
            
            CreateMap<UpdateAlerteRequest, UpdateAlerteDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<AlerteResponseDto, AlerteResponse>();
        }
    }
}