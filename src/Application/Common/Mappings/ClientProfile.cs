using AutoMapper;
using Application.Features.Clients.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<Client, ClientResponseDto>();
            CreateMap<CreateClientDto, Client>();
            CreateMap<UpdateClientDto, Client>();
        }
    }
}