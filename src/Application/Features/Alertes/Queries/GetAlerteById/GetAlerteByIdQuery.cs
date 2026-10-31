using MediatR;
using Application.Features.Alertes.Dtos;

namespace Application.Features.Alertes.Queries.GetAlerteById
{
    public record GetAlerteByIdQuery : IRequest<AlerteResponseDto>
    {
        public int Id { get; init; }
    }
}