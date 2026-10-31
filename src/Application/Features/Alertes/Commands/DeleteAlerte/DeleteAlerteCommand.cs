// Application/Features/Alertes/Commands/DeleteAlerte/DeleteAlerteCommand.cs
using MediatR;

namespace Application.Features.Alertes.Commands.DeleteAlerte
{
    public record DeleteAlerteCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}