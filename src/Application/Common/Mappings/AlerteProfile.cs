using AutoMapper;
using Application.Features.Alertes.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class AlerteProfile : Profile
    {
        public AlerteProfile()
        {
            // ============================================
            // 1. ENTITÉ → DTO DE RÉPONSE
            // ============================================
            CreateMap<Alerte, AlerteResponseDto>()
                .ForMember(dest => dest.TypeAlerte, 
                    opt => opt.MapFrom(src => src.TypeAlerte.ToString()))
                .ForMember(dest => dest.TypeAlerteLibelle, 
                    opt => opt.MapFrom(src => src.TypeAlerteLibelle))
                .ForMember(dest => dest.Niveau, 
                    opt => opt.MapFrom(src => src.Niveau.ToString()))
                .ForMember(dest => dest.NiveauLibelle, 
                    opt => opt.MapFrom(src => src.NiveauLibelle))
                .ForMember(dest => dest.UtilisateurNom, opt => opt.Ignore());

            // ============================================
            // 2. CREATE DTO → ENTITÉ
            // ============================================
            CreateMap<CreateAlerteDto, Alerte>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TypeAlerte, 
                    opt => opt.MapFrom(src => ConvertToTypeAlerte(src.TypeAlerte)))
                .ForMember(dest => dest.Niveau, 
                    opt => opt.MapFrom(src => ConvertToNiveauAlerte(src.Niveau)))
                .ForMember(dest => dest.EstLue, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.DateAlerte, opt => opt.Ignore())
                .ForMember(dest => dest.DateLecture, opt => opt.Ignore())
                .ForMember(dest => dest.UtilisateurId, opt => opt.Ignore())
                .ForMember(dest => dest.TypeAlerteLibelle, opt => opt.Ignore())
                .ForMember(dest => dest.NiveauLibelle, opt => opt.Ignore());

            // ============================================
            // 3. UPDATE DTO → ENTITÉ
            // ============================================
            CreateMap<UpdateAlerteDto, Alerte>()
                .ForMember(dest => dest.EstLue, opt => opt.MapFrom(src => src.EstLue ?? false))
                .ForMember(dest => dest.UtilisateurId, opt => opt.MapFrom(src => src.UtilisateurId))
                .ForMember(dest => dest.DateLecture, 
                    opt => opt.MapFrom(src => src.EstLue == true ? DateTime.UtcNow : (DateTime?)null))
                .ForMember(dest => dest.TypeAlerte, opt => opt.Ignore())
                .ForMember(dest => dest.Niveau, opt => opt.Ignore())
                .ForMember(dest => dest.Message, opt => opt.Ignore())
                .ForMember(dest => dest.EntiteId, opt => opt.Ignore())
                .ForMember(dest => dest.DateAlerte, opt => opt.Ignore())
                .ForMember(dest => dest.TypeAlerteLibelle, opt => opt.Ignore())
                .ForMember(dest => dest.NiveauLibelle, opt => opt.Ignore());
        }

        private static TypeAlerte ConvertToTypeAlerte(string? type)
        {
            if (string.IsNullOrEmpty(type)) return TypeAlerte.StockFaible;
            return Enum.TryParse<TypeAlerte>(type, true, out var result) ? result : TypeAlerte.StockFaible;
        }

        private static NiveauAlerte ConvertToNiveauAlerte(string? niveau)
        {
            if (string.IsNullOrEmpty(niveau)) return NiveauAlerte.Avertissement;
            return Enum.TryParse<NiveauAlerte>(niveau, true, out var result) ? result : NiveauAlerte.Avertissement;
        }
    }
}