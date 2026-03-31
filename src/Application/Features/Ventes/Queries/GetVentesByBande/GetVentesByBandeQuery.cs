using MediatR;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetVentesByBande
{
    public record GetVentesByBandeQuery : IRequest<List<VenteResponseDto>>
    {
        public int BandeId { get; init; }
    }
}