using AutoMapper;
using PresentationApi.Models.Requests.Achats;
using PresentationApi.Models.Responses;
using Application.Features.Achats.Commands.CreateAchat;
using Application.Features.Achats.Commands.UpdateAchat;
using Application.Features.Achats.Dtos;

namespace PresentationApi.Profiles
{
    public class AchatProfile : Profile
    {
        public AchatProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            CreateMap<CreateAchatRequest, CreateAchatCommand>()
                .ForMember(dest => dest.CreateAchatDto, opt => opt.MapFrom(src => src));
            
            CreateMap<CreateAchatRequest, CreateAchatDto>();
            
            CreateMap<UpdateAchatRequest, UpdateAchatCommand>()
                .ForMember(dest => dest.UpdateAchatDto, opt => opt.MapFrom(src => src));
            
            CreateMap<UpdateAchatRequest, UpdateAchatDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<AchatResponseDto, AchatResponse>();
        }
    }
}