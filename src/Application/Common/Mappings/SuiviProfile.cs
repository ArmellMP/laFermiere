// Application/Common/Mappings/SuiviProfile.cs
using AutoMapper;
using Application.Features.SuivisJournaliers.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class SuiviProfile : Profile
    {
        public SuiviProfile()
        {
            // ============================================
            // 1. ENTITÉ → DTO DE RÉPONSE
            // ============================================
            CreateMap<SuiviJournalier, SuiviResponseDto>()
                .ForMember(dest => dest.TypeSoin, 
                    opt => opt.MapFrom(src => src.TypeSoin.HasValue ? src.TypeSoin.ToString() : null))
                .ForMember(dest => dest.BandeCode, opt => opt.Ignore())
                .ForMember(dest => dest.StockConsommeNom, opt => opt.Ignore())
                .ForMember(dest => dest.UniteMesureStock, opt => opt.Ignore())
                .ForMember(dest => dest.CreateurNom, opt => opt.Ignore())
                .ForMember(dest => dest.DateArriveeBande, opt => opt.Ignore())
                .ForMember(dest => dest.PeseeEffectuee, 
                    opt => opt.MapFrom(src => src.PeseeEffectuee))
                .ForMember(dest => dest.TypeSoinLibelle, opt => opt.Ignore())
                .ForMember(dest => dest.TemperatureAvis, opt => opt.Ignore())
                .ForMember(dest => dest.AgeEnJours, opt => opt.Ignore());

            // ============================================
            // 2. CREATE DTO → ENTITÉ
            // ============================================
            CreateMap<CreateSuiviDto, SuiviJournalier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TypeSoin, 
                    opt => opt.MapFrom(src => ConvertStringToTypeSoin(src.TypeSoin)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PeseeEffectuee, opt => opt.Ignore())
                .ForMember(dest => dest.SoinEffectue, opt => opt.Ignore())
                .ForMember(dest => dest.StockConsomme, opt => opt.Ignore())
                .ForMember(dest => dest.TemperatureAnormale, opt => opt.Ignore());

            // ============================================
            // 3. UPDATE DTO → ENTITÉ
            // ============================================
            CreateMap<UpdateSuiviDto, SuiviJournalier>()
                .ForMember(dest => dest.TypeSoin, 
                    opt => opt.MapFrom(src => ConvertStringToTypeSoin(src.TypeSoin)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PeseeEffectuee, opt => opt.Ignore())
                .ForMember(dest => dest.SoinEffectue, opt => opt.Ignore())
                .ForMember(dest => dest.StockConsomme, opt => opt.Ignore())
                .ForMember(dest => dest.TemperatureAnormale, opt => opt.Ignore());
        }

        // Méthode utilitaire pour la conversion string → TypeSoin
        private static TypeSoin? ConvertStringToTypeSoin(string? typeSoin)
        {
            if (string.IsNullOrEmpty(typeSoin)) return null;
            return Enum.TryParse<TypeSoin>(typeSoin, true, out var result) ? result : null;
        }
    }
}