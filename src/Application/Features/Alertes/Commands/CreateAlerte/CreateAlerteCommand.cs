using MediatR;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Commands.CreateAlerte
{
    public record CreateAlerteCommand : IRequest<AlerteResponseDto>
    {
        public CreateAlerteDto CreateAlerteDto { get; init; } = null!;
    }
}