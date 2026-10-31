using AutoMapper;
using Application.Features.Achats.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class AchatProfile : Profile
    {
        public AchatProfile()
        {
            // ============================================
            // 1. ENTITÉ → DTO DE RÉPONSE
            // ============================================
            CreateMap<Achat, AchatResponseDto>()
                .ForMember(dest => dest.TypeAchat, opt => opt.MapFrom(src => src.TypeAchat.ToString()))
                .ForMember(dest => dest.TypeAchatLibelle, opt => opt.MapFrom(src => GetTypeAchatLibelle(src.TypeAchat)))
                .ForMember(dest => dest.UniteMesure, opt => opt.MapFrom(src => src.UniteMesure.ToString()))
                .ForMember(dest => dest.UniteMesureLibelle, opt => opt.MapFrom(src => GetUniteMesureAchatLibelle(src.UniteMesure)))
                .ForMember(dest => dest.FournisseurNom, opt => opt.Ignore())
                .ForMember(dest => dest.BandeCode, opt => opt.Ignore())
                .ForMember(dest => dest.CreateurNom, opt => opt.Ignore())
                .ForMember(dest => dest.EstRecu, opt => opt.MapFrom(src => src.EstRecu))
                .ForMember(dest => dest.JoursEntreAchatEtReception, opt => opt.MapFrom(src => src.JoursEntreAchatEtReception));

            // ============================================
            // 2. CREATE DTO → ENTITÉ
            // ============================================
            CreateMap<CreateAchatDto, Achat>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TypeAchat, opt => opt.MapFrom(src => ConvertToTypeAchat(src.TypeAchat)))
                .ForMember(dest => dest.UniteMesure, opt => opt.MapFrom(src => ConvertToUniteMesureAchat(src.UniteMesure)))
                .ForMember(dest => dest.MontantTotal, opt => opt.MapFrom(src => src.Quantite * src.PrixUnitaire))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // ============================================
            // 3. UPDATE DTO → ENTITÉ
            // ============================================
            CreateMap<UpdateAchatDto, Achat>()
                .ForMember(dest => dest.TypeAchat, opt => opt.MapFrom(src => ConvertToTypeAchat(src.TypeAchat)))
                .ForMember(dest => dest.UniteMesure, opt => opt.MapFrom(src => ConvertToUniteMesureAchat(src.UniteMesure)))
                .ForMember(dest => dest.MontantTotal, opt => opt.MapFrom(src => 
                    src.Quantite.HasValue && src.PrixUnitaire.HasValue 
                        ? src.Quantite.Value * src.PrixUnitaire.Value 
                        : 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }

        // ============================================
        // MÉTHODES DE CONVERSION
        // ============================================

        private static TypeAchat ConvertToTypeAchat(string? type)
        {
            if (string.IsNullOrEmpty(type)) return TypeAchat.Autre;
            return Enum.TryParse<TypeAchat>(type, true, out var result) ? result : TypeAchat.Autre;
        }

        private static UniteMesureAchat ConvertToUniteMesureAchat(string? unite)
        {
            if (string.IsNullOrEmpty(unite)) return UniteMesureAchat.Unite;
            return Enum.TryParse<UniteMesureAchat>(unite, true, out var result) ? result : UniteMesureAchat.Unite;
        }

        private static string GetTypeAchatLibelle(TypeAchat type)
        {
            return type switch
            {
                TypeAchat.Poussins => "Poussins",
                TypeAchat.Aliment => "Aliment",
                TypeAchat.Medicament => "Médicament",
                TypeAchat.Materiel => "Matériel",
                TypeAchat.Services => "Services",
                TypeAchat.Transport => "Transport",
                TypeAchat.Autre => "Autre",
                _ => "Inconnu"
            };
        }

        private static string GetUniteMesureAchatLibelle(UniteMesureAchat unite)
        {
            return unite switch
            {
                UniteMesureAchat.Unite => "Unité",
                UniteMesureAchat.Kg => "Kilogramme",
                UniteMesureAchat.L => "Litre",
                UniteMesureAchat.Sac => "Sac",
                UniteMesureAchat.Dose => "Dose",
                UniteMesureAchat.Heure => "Heure",
                UniteMesureAchat.Km => "Kilomètre",
                UniteMesureAchat.Paquet => "Paquet",
                _ => unite.ToString()
            };
        }
    }
}