using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Queries.GetAllBandes
{
    public record GetAllBandesQuery : IRequest<List<BandeResponseDto>>
    {
        /// <summary>
        /// Inclure les détails supplémentaires (mortalités, jours, etc.). Par défaut : false.
        /// </summary>
        public bool IncludeDetails { get; init; } = false;
    }
}