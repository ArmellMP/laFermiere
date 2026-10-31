using AutoMapper;
using PresentationApi.Models.Requests.Parametres;
using PresentationApi.Models.Responses;
using Application.Features.Parametres.Commands.CreateParametre;
using Application.Features.Parametres.Commands.UpdateParametre;
using Application.Features.Parametres.Dtos;

namespace PresentationApi.Profiles
{
    public class ParametreProfile : Profile
    {
        public ParametreProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            CreateMap<CreateParametreRequest, CreateParametreCommand>()
                .ForMember(dest => dest.CreateParametreDto, opt => opt.MapFrom(src => src));
            
            CreateMap<CreateParametreRequest, CreateParametreDto>();
            
            CreateMap<UpdateParametreRequest, UpdateParametreCommand>()
                .ForMember(dest => dest.UpdateParametreDto, opt => opt.MapFrom(src => src));
            
            CreateMap<UpdateParametreRequest, UpdateParametreDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<ParametreResponseDto, ParametreResponse>();
        }
    }
}