using MediatR;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetVentesByClient
{
    public record GetVentesByClientQuery : IRequest<List<VenteResponseDto>>
    {
        public int ClientId { get; init; }
    }
}