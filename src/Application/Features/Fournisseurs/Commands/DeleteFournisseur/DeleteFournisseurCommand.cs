using MediatR;

namespace Application.Features.Fournisseurs.Commands.DeleteFournisseur
{
    public record DeleteFournisseurCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }
}