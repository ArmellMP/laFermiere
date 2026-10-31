using MediatR;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAchatsByBande
{
    public record GetAchatsByBandeQuery : IRequest<List<AchatResponseDto>>
    {
        public int BandeId { get; init; }
    }
}