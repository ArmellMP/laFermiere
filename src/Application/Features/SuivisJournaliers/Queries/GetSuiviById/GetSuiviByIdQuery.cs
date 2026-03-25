using MediatR;
using Application.Features.SuivisJournaliers.Dtos;

namespace Application.Features.SuivisJournaliers.Queries.GetSuiviById
{
    public record GetSuiviByIdQuery : IRequest<SuiviResponseDto>
    {
        /// <summary>
        /// ID du suivi à récupérer
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Optionnel : Inclure les détails supplémentaires
        /// </summary>
        public bool IncludeDetails { get; init; }
    }
}