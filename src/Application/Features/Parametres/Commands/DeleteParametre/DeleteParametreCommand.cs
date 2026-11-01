using MediatR;

namespace Application.Features.Parametres.Commands.DeleteParametre
{
    public record DeleteParametreCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}