using AutoMapper;
using PresentationApi.Models.Requests.Suivis;
using PresentationApi.Models.Responses;
using Application.Features.SuivisJournaliers.Commands.CreateSuivi;
using Application.Features.SuivisJournaliers.Commands.UpdateSuivi;
using Application.Features.SuivisJournaliers.Dtos;

namespace PresentationApi.Profiles
{
    public class SuiviProfile : Profile
    {
        public SuiviProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            // CreateSuiviRequest → CreateSuiviCommand
            CreateMap<CreateSuiviRequest, CreateSuiviCommand>()
                .ForMember(dest => dest.CreateSuiviDto, opt => opt.MapFrom(src => src));
            
            // CreateSuiviRequest → CreateSuiviDto
            CreateMap<CreateSuiviRequest, CreateSuiviDto>();
            
            // UpdateSuiviRequest → UpdateSuiviCommand
            CreateMap<UpdateSuiviRequest, UpdateSuiviCommand>()
                .ForMember(dest => dest.UpdateSuiviDto, opt => opt.MapFrom(src => src));
            
            // UpdateSuiviRequest → UpdateSuiviDto
            CreateMap<UpdateSuiviRequest, UpdateSuiviDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<SuiviResponseDto, SuiviResponse>()
                .ForMember(dest => dest.PeseeEffectuee, 
                    opt => opt.MapFrom(src => src.PeseeEffectuee))
                .ForMember(dest => dest.TypeSoinLibelle, 
                    opt => opt.MapFrom(src => src.TypeSoinLibelle))
                .ForMember(dest => dest.TemperatureAvis, 
                    opt => opt.MapFrom(src => src.TemperatureAvis))
                .ForMember(dest => dest.AgeEnJours, 
                    opt => opt.MapFrom(src => src.AgeEnJours));
        }
    }
}