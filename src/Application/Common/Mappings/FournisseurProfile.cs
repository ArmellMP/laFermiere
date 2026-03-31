using AutoMapper;
using Application.Features.Fournisseurs.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class FournisseurProfile : Profile
    {
        public FournisseurProfile()
        {
            CreateMap<Fournisseur, FournisseurResponseDto>()
                .ForMember(dest => dest.TypeFournisseur, opt => opt.MapFrom(src => src.TypeFournisseur.ToString()))
                .ForMember(dest => dest.TypeFournisseurLibelle, opt => opt.MapFrom(src => src.TypeFournisseurLibelle))
                .ForMember(dest => dest.NombreAchats, opt => opt.Ignore())
                .ForMember(dest => dest.MontantTotalAchats, opt => opt.Ignore());

            CreateMap<CreateFournisseurDto, Fournisseur>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TypeFournisseur, opt => opt.MapFrom(src => ConvertToTypeFournisseur(src.TypeFournisseur)))
                .ForMember(dest => dest.EstActif, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateFournisseurDto, Fournisseur>()
                .ForMember(dest => dest.TypeFournisseur, opt => opt.MapFrom(src => ConvertToTypeFournisseur(src.TypeFournisseur, src.TypeFournisseur != null)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }

        private static TypeFournisseur ConvertToTypeFournisseur(string? type, bool hasValue = true)
        {
            if (!hasValue || string.IsNullOrEmpty(type))
            {
                return TypeFournisseur.Autre;
            }
            return Enum.TryParse<TypeFournisseur>(type, true, out var result) ? result : TypeFournisseur.Autre;
        }
    }
}



// using AutoMapper;
// using Application.Features.Fournisseurs.Dtos;
// using Domain.Entities;
// using Domain.Enums;

// namespace Application.Common.Mappings
// {
//     public class FournisseurProfile : Profile
//     {
//         public FournisseurProfile()
//         {
//             // ============================================
//             // 1. ENTITÉ → DTO DE RÉPONSE
//             // ============================================
//             CreateMap<Fournisseur, FournisseurResponseDto>()
//                 .ForMember(dest => dest.TypeFournisseur, 
//                     opt => opt.MapFrom(src => src.TypeFournisseur.ToString()))
//                 .ForMember(dest => dest.TypeFournisseurLibelle, 
//                     opt => opt.MapFrom(src => src.TypeFournisseurLibelle))
//                 .ForMember(dest => dest.NombreAchats, 
//                     opt => opt.Ignore())
//                 .ForMember(dest => dest.MontantTotalAchats, 
//                     opt => opt.Ignore());

//             // ============================================
//             // 2. CREATE DTO → ENTITÉ
//             // ============================================
//             CreateMap<CreateFournisseurDto, Fournisseur>()
//                 .ForMember(dest => dest.Id, opt => opt.Ignore())
//                 .ForMember(dest => dest.TypeFournisseur, 
//                     opt => opt.MapFrom(src => Enum.Parse<TypeFournisseur>(src.TypeFournisseur, true)))
//                 .ForMember(dest => dest.EstActif, opt => opt.MapFrom(src => true))
//                 .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
//                 .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
//                 //.ForMember(dest => dest.TypeFournisseurLibelle, opt => opt.Ignore())
//                 //.ForMember(dest => dest.NombreAchats, opt => opt.Ignore())
//                 //.ForMember(dest => dest.MontantTotalAchats, opt => opt.Ignore())
                

//             // ============================================
//             // 3. UPDATE DTO → ENTITÉ
//             // ============================================
//             CreateMap<UpdateFournisseurDto, Fournisseur>()
//                 .ForMember(dest => dest.TypeFournisseur, 
//                     opt => opt.MapFrom(src => src.TypeFournisseur != null 
//                         ? Enum.Parse<TypeFournisseur>(src.TypeFournisseur, true) 
//                         : dest.TypeFournisseur))
//                 .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
//                 .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
//                 //.ForMember(dest => dest.TypeFournisseurLibelle, opt => opt.Ignore())
//                 //.ForMember(dest => dest.NombreAchats, opt => opt.Ignore())
//                 //.ForMember(dest => dest.MontantTotalAchats, opt => opt.Ignore());
//         }
//     }
// }