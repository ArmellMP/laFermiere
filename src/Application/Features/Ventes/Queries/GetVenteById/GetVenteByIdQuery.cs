using MediatR;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetVenteById
{
    public record GetVenteByIdQuery : IRequest<VenteResponseDto>
    {
        public int Id { get; init; }
    }
}