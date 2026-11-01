using MediatR;

namespace Application.Features.Alertes.Commands.MarquerAlertes
{
    public record MarquerAlertesCommand : IRequest<Unit>
    {
        public IEnumerable<int> AlerteIds { get; init; } = new List<int>();
        public int UtilisateurId { get; init; }
        public bool MarquerToutes { get; init; }
    }
}