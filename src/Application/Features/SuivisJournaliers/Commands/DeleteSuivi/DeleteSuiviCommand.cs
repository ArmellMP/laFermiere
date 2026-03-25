using MediatR;

namespace Application.Features.SuivisJournaliers.Commands.DeleteSuivi
{
    public record DeleteSuiviCommand : IRequest<Unit>
    {
        /// <summary>
        /// ID du suivi à supprimer.
        /// </summary>
        public int Id { get; init; }
    }
}