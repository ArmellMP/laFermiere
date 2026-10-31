using AutoMapper;
using Application.Features.Parametres.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class ParametreProfile : Profile
    {
        public ParametreProfile()
        {
            // ============================================
            // 1. ENTITÉ → DTO DE RÉPONSE
            // ============================================
            CreateMap<Parametre, ParametreResponseDto>()
                .ForMember(dest => dest.Categorie, 
                    opt => opt.MapFrom(src => src.Categorie.ToString()))
                .ForMember(dest => dest.CategorieLibelle, 
                    opt => opt.MapFrom(src => src.CategorieLibelle));

            // ============================================
            // 2. CREATE DTO → ENTITÉ
            // ============================================
            CreateMap<CreateParametreDto, Parametre>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Categorie, 
                    opt => opt.MapFrom(src => ConvertToCategorie(src.Categorie)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CategorieLibelle, opt => opt.Ignore());

            // ============================================
            // 3. UPDATE DTO → ENTITÉ
            // ============================================
            CreateMap<UpdateParametreDto, Parametre>()
                .ForMember(dest => dest.Categorie, 
                    opt => opt.MapFrom(src => ConvertToCategorie(src.Categorie)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CategorieLibelle, opt => opt.Ignore());
        }

        private static CategorieParametre ConvertToCategorie(string? categorie)
        {
            if (string.IsNullOrEmpty(categorie)) return CategorieParametre.General;
            return Enum.TryParse<CategorieParametre>(categorie, true, out var result) 
                ? result 
                : CategorieParametre.General;
        }
    }
}