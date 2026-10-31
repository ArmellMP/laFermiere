using MediatR;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Commands.UpdateAlerte
{
    public record UpdateAlerteCommand : IRequest<AlerteResponseDto>
    {
        public UpdateAlerteDto UpdateAlerteDto { get; init; } = null!;
    }
}