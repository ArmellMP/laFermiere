using MediatR;

namespace Application.Features.Ventes.Commands.DeleteVente
{
    public record DeleteVenteCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}