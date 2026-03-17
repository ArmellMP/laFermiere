using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Commands.CloturerBande;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    public class BandeProfile : Profile
    {
        public BandeProfile()
        {
            // ============================================
            // 1. MAPPING ENTITÉ → DTO DE RÉPONSE
            // ============================================
            
            // Bande → BandeResponseDto
            CreateMap<Bande, BandeResponseDto>()
                .ForMember(dest => dest.SuperficieCalculee, 
                    opt => opt.MapFrom(src => src.SuperficieCalculee))
                .ForMember(dest => dest.IncludeDetails, 
                    opt => opt.Ignore()) // Sera défini dans les handlers
                .ForMember(dest => dest.TotalMortalites,
                    opt => opt.Ignore()) // Calculé conditionnellement
                .ForMember(dest => dest.JoursEcoules,
                    opt => opt.Ignore())
                .ForMember(dest => dest.JoursRestants,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PourcentageAvancement,
                    opt => opt.Ignore());
            
            // ============================================
            // 2. MAPPING POUR LA CRÉATION (CreateBandeDto → Bande)
            // ============================================
            
            CreateMap<CreateBandeDto, Bande>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID auto-généré
                .ForMember(dest => dest.Code, 
                    opt => opt.MapFrom(src => src.Code)) // Peut être null (généré dans handler)
                .ForMember(dest => dest.DateArrivee, 
                    opt => opt.MapFrom(src => src.DateArrivee))
                .ForMember(dest => dest.NbPoussinsInit, 
                    opt => opt.MapFrom(src => src.NbPoussinsInit))
                .ForMember(dest => dest.NbPoussinsActuel, 
                    opt => opt.MapFrom(src => src.NbPoussinsInit)) // Au départ = initial
                .ForMember(dest => dest.Observations, 
                    opt => opt.MapFrom(src => src.Observations))
                .ForMember(dest => dest.Statut, 
                    opt => opt.Ignore()) // Défini dans le handler
                .ForMember(dest => dest.DateCloture, 
                    opt => opt.Ignore()) // Null à la création
                .ForMember(dest => dest.CreatedAt, 
                    opt => opt.Ignore()) // Défini dans le handler
                .ForMember(dest => dest.UpdatedAt, 
                    opt => opt.Ignore()) // Défini dans le handler
                .ForMember(dest => dest.SuperficieCalculee, 
                    opt => opt.Ignore()); // Propriété calculée dans l'entité
            
            // ============================================
            // 3. MAPPING POUR LA MISE À JOUR (UpdateBandeDto → Bande)
            // ============================================
            
            CreateMap<UpdateBandeDto, Bande>()
                .ForMember(dest => dest.Id, 
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NbPoussinsActuel, 
                    opt => opt.MapFrom(src => src.NbPoussinsActuel))
                .ForMember(dest => dest.Observations, 
                    opt => opt.MapFrom(src => src.Observations))
                .ForMember(dest => dest.Statut, 
                    opt => opt.MapFrom(src => src.Statut))
                .ForMember(dest => dest.DateCloture, 
                    opt => opt.MapFrom(src => src.DateCloture))
                .ForMember(dest => dest.UpdatedAt, 
                    opt => opt.Ignore()) // Défini dans le handler
                
                // Propriétés NON modifiables (on ignore ou on garde les valeurs existantes)
                .ForMember(dest => dest.Code, 
                    opt => opt.Ignore()) // Ne change pas en update
                .ForMember(dest => dest.DateArrivee, 
                    opt => opt.Ignore()) // Ne change pas
                .ForMember(dest => dest.NbPoussinsInit, 
                    opt => opt.Ignore()) // Ne change pas
                .ForMember(dest => dest.CreatedAt, 
                    opt => opt.Ignore()) // Ne change pas
                .ForMember(dest => dest.SuperficieCalculee, 
                    opt => opt.Ignore()); // Propriété calculée
            
            // ============================================
            // 4. MAPPING POUR LES AUTRES COMMANDES 
            // ============================================
            
            CreateMap<CloturerBandeCommand, Bande>()
                .ForMember(dest => dest.Statut, opt => opt.MapFrom(src => StatutBande.Cloturee))
                .ForMember(dest => dest.DateCloture, opt => opt.MapFrom(src => src.DateCloture ?? DateTime.Now));
        }
    }
}