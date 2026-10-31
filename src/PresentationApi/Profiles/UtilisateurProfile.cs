using AutoMapper;
using PresentationApi.Models.Requests.Utilisateurs;
using PresentationApi.Models.Responses;
using Application.Features.Utilisateurs.Commands.CreateUtilisateur;
using Application.Features.Utilisateurs.Commands.UpdateUtilisateur;
using Application.Features.Utilisateurs.Dtos;
using Application.Features.Utilisateurs.Queries.AuthenticateUser;

namespace PresentationApi.Profiles
{
    public class UtilisateurProfile : Profile
    {
        public UtilisateurProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            CreateMap<CreateUtilisateurRequest, CreateUtilisateurCommand>()
                .ForMember(dest => dest.CreateUtilisateurDto, opt => opt.MapFrom(src => src));
            
            CreateMap<CreateUtilisateurRequest, CreateUtilisateurDto>();
            
            CreateMap<UpdateUtilisateurRequest, UpdateUtilisateurCommand>()
                .ForMember(dest => dest.UpdateUtilisateurDto, opt => opt.MapFrom(src => src));
            
            CreateMap<UpdateUtilisateurRequest, UpdateUtilisateurDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<UtilisateurResponseDto, UtilisateurResponse>();
        }
    }
}