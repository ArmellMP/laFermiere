using AutoMapper;
using Application.Features.Utilisateurs.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class UtilisateurProfile : Profile
    {
        public UtilisateurProfile()
        {
            // ============================================
            // 1. ENTITÉ → DTO DE RÉPONSE
            // ============================================
            CreateMap<Utilisateur, UtilisateurResponseDto>()
                .ForMember(dest => dest.Role, 
                    opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.RoleLibelle, 
                    opt => opt.MapFrom(src => src.RoleLibelle))
                .ForMember(dest => dest.EstAdmin, 
                    opt => opt.MapFrom(src => src.EstAdmin))
                .ForMember(dest => dest.EstFermier, 
                    opt => opt.MapFrom(src => src.EstFermier))
                .ForMember(dest => dest.EstVeterinaire, 
                    opt => opt.MapFrom(src => src.EstVeterinaire))
                .ForMember(dest => dest.EstComptable, 
                    opt => opt.MapFrom(src => src.EstComptable));

            // ============================================
            // 2. CREATE DTO → ENTITÉ
            // ============================================
            CreateMap<CreateUtilisateurDto, Utilisateur>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, 
                    opt => opt.MapFrom(src => ConvertToRole(src.Role)))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.EstActif, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.DerniereConnexion, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RoleLibelle, opt => opt.Ignore())
                .ForMember(dest => dest.EstAdmin, opt => opt.Ignore())
                .ForMember(dest => dest.EstFermier, opt => opt.Ignore())
                .ForMember(dest => dest.EstVeterinaire, opt => opt.Ignore())
                .ForMember(dest => dest.EstComptable, opt => opt.Ignore());

            // ============================================
            // 3. UPDATE DTO → ENTITÉ
            // ============================================
            CreateMap<UpdateUtilisateurDto, Utilisateur>()
                .ForMember(dest => dest.Role, 
                    opt => opt.MapFrom(src => ConvertToRole(src.Role)))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RoleLibelle, opt => opt.Ignore())
                .ForMember(dest => dest.EstAdmin, opt => opt.Ignore())
                .ForMember(dest => dest.EstFermier, opt => opt.Ignore())
                .ForMember(dest => dest.EstVeterinaire, opt => opt.Ignore())
                .ForMember(dest => dest.EstComptable, opt => opt.Ignore());
        }

        private static RoleUtilisateur ConvertToRole(string? role)
        {
            if (string.IsNullOrEmpty(role)) return RoleUtilisateur.Fermier;
            return Enum.TryParse<RoleUtilisateur>(role, true, out var result) ? result : RoleUtilisateur.Fermier;
        }
    }
}