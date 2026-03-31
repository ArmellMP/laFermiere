// PresentationApi/Profiles/VenteProfile.cs
using AutoMapper;
using PresentationApi.Models.Requests.Ventes;
using PresentationApi.Models.Responses;
using Application.Features.Ventes.Commands.CreateVente;
using Application.Features.Ventes.Commands.UpdateVente;
using Application.Features.Ventes.Dtos;

namespace PresentationApi.Profiles
{
    public class VenteProfile : Profile
    {
        public VenteProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            CreateMap<CreateVenteRequest, CreateVenteCommand>()
                .ForMember(dest => dest.CreateVenteDto, opt => opt.MapFrom(src => src));
            
            CreateMap<CreateVenteRequest, CreateVenteDto>();
            
            CreateMap<UpdateVenteRequest, UpdateVenteCommand>()
                .ForMember(dest => dest.UpdateVenteDto, opt => opt.MapFrom(src => src));
            
            CreateMap<UpdateVenteRequest, UpdateVenteDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<VenteResponseDto, VenteResponse>();
        }
    }
}