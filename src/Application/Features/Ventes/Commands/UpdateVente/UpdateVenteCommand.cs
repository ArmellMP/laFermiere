using MediatR;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Commands.UpdateVente
{
    public record UpdateVenteCommand : IRequest<VenteResponseDto>
    {
        public UpdateVenteDto UpdateVenteDto { get; init; } = null!;
    }
}