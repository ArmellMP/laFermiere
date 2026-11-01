using MediatR;

namespace Application.Features.Achats.Commands.DeleteAchat
{
    public record DeleteAchatCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}