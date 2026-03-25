using AutoMapper;
using PresentationApi.Models.Requests.Clients;
using PresentationApi.Models.Responses;
using Application.Features.Clients.Commands.CreateClient;
using Application.Features.Clients.Commands.UpdateClient;
using Application.Features.Clients.Dtos;

namespace PresentationApi.Profiles
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            // ============================================
            // 1. REQUESTS → COMMANDS
            // ============================================
            
            // CreateClientRequest → CreateClientCommand
            CreateMap<CreateClientRequest, CreateClientCommand>()
                .ForMember(dest => dest.CreateClientDto, opt => opt.MapFrom(src => src));
            
            // CreateClientRequest → CreateClientDto
            CreateMap<CreateClientRequest, CreateClientDto>();
            
            // UpdateClientRequest → UpdateClientCommand
            CreateMap<UpdateClientRequest, UpdateClientCommand>()
                .ForMember(dest => dest.UpdateClientDto, opt => opt.MapFrom(src => src));
            
            // UpdateClientRequest → UpdateClientDto
            CreateMap<UpdateClientRequest, UpdateClientDto>();
            
            // ============================================
            // 2. APPLICATION DTOS → API RESPONSES
            // ============================================
            
            CreateMap<ClientResponseDto, ClientResponse>();
        }
    }
}