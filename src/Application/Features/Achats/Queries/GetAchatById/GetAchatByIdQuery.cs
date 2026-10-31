using MediatR;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAchatById
{
    public record GetAchatByIdQuery : IRequest<AchatResponseDto>
    {
        public int Id { get; init; }
    }
}