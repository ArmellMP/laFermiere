using AutoMapper;
using Application.Features.Ventes.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class VenteProfile : Profile
    {
        public VenteProfile()
        {
            // ============================================
            // 1. ENTITÉ → DTO DE RÉPONSE
            // ============================================
            CreateMap<Vente, VenteResponseDto>()
                .ForMember(dest => dest.TypeVente, opt => opt.MapFrom(src => src.TypeVente.ToString()))
                .ForMember(dest => dest.TypeVenteLibelle, opt => opt.MapFrom(src => GetTypeVenteLibelle(src.TypeVente)))
                .ForMember(dest => dest.UniteMesure, opt => opt.MapFrom(src => src.UniteMesure.ToString()))
                .ForMember(dest => dest.UniteMesureLibelle, opt => opt.MapFrom(src => GetUniteMesureVenteLibelle(src.UniteMesure)))
                .ForMember(dest => dest.ModePaiement, opt => opt.MapFrom(src => src.ModePaiement.ToString()))
                .ForMember(dest => dest.ModePaiementLibelle, opt => opt.MapFrom(src => GetModePaiementLibelle(src.ModePaiement)))
                .ForMember(dest => dest.BandeCode, opt => opt.Ignore())
                .ForMember(dest => dest.ClientNom, opt => opt.Ignore())
                .ForMember(dest => dest.CreateurNom, opt => opt.Ignore());

            // ============================================
            // 2. CREATE DTO → ENTITÉ
            // ============================================
            CreateMap<CreateVenteDto, Vente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TypeVente, opt => opt.MapFrom(src => ConvertToTypeVente(src.TypeVente)))
                .ForMember(dest => dest.UniteMesure, opt => opt.MapFrom(src => ConvertToUniteMesureVente(src.UniteMesure)))
                .ForMember(dest => dest.ModePaiement, opt => opt.MapFrom(src => ConvertToModePaiement(src.ModePaiement)))
                .ForMember(dest => dest.MontantTotal, opt => opt.MapFrom(src => src.Quantite * src.PrixUnitaire))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // ============================================
            // 3. UPDATE DTO → ENTITÉ
            // ============================================
            CreateMap<UpdateVenteDto, Vente>()
                .ForMember(dest => dest.TypeVente, opt => opt.MapFrom(src => ConvertToTypeVente(src.TypeVente)))
                .ForMember(dest => dest.UniteMesure, opt => opt.MapFrom(src => ConvertToUniteMesureVente(src.UniteMesure)))
                .ForMember(dest => dest.ModePaiement, opt => opt.MapFrom(src => ConvertToModePaiement(src.ModePaiement)))
                .ForMember(dest => dest.MontantTotal, opt => opt.MapFrom(src => src.Quantite.HasValue && src.PrixUnitaire.HasValue 
                    ? src.Quantite.Value * src.PrixUnitaire.Value 
                    : 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }

        private static TypeVente ConvertToTypeVente(string? type)
        {
            if (string.IsNullOrEmpty(type)) return TypeVente.Autres;
            return Enum.TryParse<TypeVente>(type, true, out var result) ? result : TypeVente.Autres;
        }

        private static UniteMesureVente ConvertToUniteMesureVente(string? unite)
        {
            if (string.IsNullOrEmpty(unite)) return UniteMesureVente.Unite;
            return Enum.TryParse<UniteMesureVente>(unite, true, out var result) ? result : UniteMesureVente.Unite;
        }

        private static ModePaiement ConvertToModePaiement(string? mode)
        {
            if (string.IsNullOrEmpty(mode)) return ModePaiement.Especes;
            return Enum.TryParse<ModePaiement>(mode, true, out var result) ? result : ModePaiement.Especes;
        }

        private static string GetTypeVenteLibelle(TypeVente type)
        {
            return type switch
            {
                TypeVente.PouletsVivants => "Poulets vivants",
                TypeVente.PouletsNettoyes => "Poulets nettoyés",
                TypeVente.PouletsFumes => "Poulets fumés",
                TypeVente.Fientes => "Fientes",
                TypeVente.Autres => "Autres",
                _ => "Inconnu"
            };
        }

        private static string GetUniteMesureVenteLibelle(UniteMesureVente unite)
        {
            return unite switch
            {
                UniteMesureVente.Unite => "Unité",
                UniteMesureVente.Kg => "Kilogramme",
                UniteMesureVente.Sac => "Sac",
                UniteMesureVente.Camion => "Camion",
                UniteMesureVente.Rouleau => "Rouleau",
                UniteMesureVente.Paquet => "Paquet",
                _ => unite.ToString()
            };
        }

        private static string GetModePaiementLibelle(ModePaiement mode)
        {
            return mode switch
            {
                ModePaiement.Especes => "Espèces",
                ModePaiement.PaiementMobile => "Paiement Mobile",
                ModePaiement.VirementBancaire => "Virement bancaire",
                ModePaiement.Cheque => "Chèque",
                ModePaiement.Credit => "Crédit",
                _ => mode.ToString()
            };
        }
    }
}