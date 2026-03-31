using MediatR;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Commands.CreateVente
{
    public record CreateVenteCommand : IRequest<VenteResponseDto>
    {
        public CreateVenteDto CreateVenteDto { get; init; } = null!;
    }
}