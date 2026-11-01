using MediatR;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Commands.UpdateAchat
{
    public record UpdateAchatCommand : IRequest<AchatResponseDto>
    {
        public UpdateAchatDto UpdateAchatDto { get; init; } = null!;
    }
}