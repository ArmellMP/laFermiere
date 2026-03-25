using MediatR;
using Application.Features.SuivisJournaliers.Dtos;

namespace Application.Features.SuivisJournaliers.Commands.CreateSuivi
{
    /// <summary>
    /// Commande pour créer un nouveau suivi journalier.
    /// </summary>
    public record CreateSuiviCommand : IRequest<SuiviResponseDto>
    {
        /// <summary>
        /// Données de création du suivi.
        /// </summary>
        public CreateSuiviDto CreateSuiviDto { get; init; } = null!;
    }
}