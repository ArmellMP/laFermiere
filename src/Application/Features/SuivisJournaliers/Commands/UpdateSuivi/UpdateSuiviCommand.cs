using MediatR;
using Application.Features.SuivisJournaliers.Dtos;

namespace Application.Features.SuivisJournaliers.Commands.UpdateSuivi
{
    public record UpdateSuiviCommand : IRequest<SuiviResponseDto>
    {
        /// <summary>
        /// Données de mise à jour du suivi.
        /// </summary>
        public UpdateSuiviDto UpdateSuiviDto { get; init; } = null!;
    }
}