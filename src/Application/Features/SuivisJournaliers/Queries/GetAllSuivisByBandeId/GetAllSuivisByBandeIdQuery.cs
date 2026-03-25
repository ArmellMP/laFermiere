
using MediatR;
using Application.Features.SuivisJournaliers.Dtos;

namespace Application.Features.SuivisJournaliers.Queries.GetAllSuivisByBandeId
{
    /// <summary>
    /// Query pour récupérer tous les suivis d'une bande spécifique. Triés par jour pour suivre la progression.
    /// </summary>
    public record GetAllSuivisByBandeIdQuery : IRequest<List<SuiviResponseDto>>
    {
        /// <summary>
        /// ID de la bande dont on veut les suivis
        /// </summary>
        public int BandeId { get; init; }

        /// <summary>
        /// Optionnel : Filtrer par date de début
        /// </summary>
        public DateTime? DateDebut { get; init; }

        /// <summary>
        /// Optionnel : Filtrer par date de fin
        /// </summary>
        public DateTime? DateFin { get; init; }

        /// <summary>
        /// Optionnel : Inclure uniquement les suivis avec soins
        /// </summary>
        public bool OnlyWithSoins { get; init; }

        /// <summary>
        /// Optionnel : Inclure uniquement les suivis avec mortalités
        /// </summary>
        public bool OnlyWithMortalites { get; init; }
    }
}