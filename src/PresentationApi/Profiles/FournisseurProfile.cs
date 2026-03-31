using AutoMapper;
using PresentationApi.Models.Requests.Fournisseurs;
using PresentationApi.Models.Responses;
using Application.Features.Fournisseurs.Commands.CreateFournisseur;
using Application.Features.Fournisseurs.Commands.UpdateFournisseur;
using Application.Features.Fournisseurs.Dtos;

namespace PresentationApi.Profiles
{
    public class FournisseurProfile : Profile
    {
        public FournisseurProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            CreateMap<CreateFournisseurRequest, CreateFournisseurCommand>()
                .ForMember(dest => dest.CreateFournisseurDto, opt => opt.MapFrom(src => src));
            
            CreateMap<CreateFournisseurRequest, CreateFournisseurDto>();
            
            CreateMap<UpdateFournisseurRequest, UpdateFournisseurCommand>()
                .ForMember(dest => dest.UpdateFournisseurDto, opt => opt.MapFrom(src => src));
            
            CreateMap<UpdateFournisseurRequest, UpdateFournisseurDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<FournisseurResponseDto, FournisseurResponse>();
        }
    }
}