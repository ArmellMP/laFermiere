using MediatR;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Commands.CreateAchat
{
    public record CreateAchatCommand : IRequest<AchatResponseDto>
    {
        public CreateAchatDto CreateAchatDto { get; init; } = null!;
    }
}