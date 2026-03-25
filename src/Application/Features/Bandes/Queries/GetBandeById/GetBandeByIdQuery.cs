using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Queries.GetBandeById
{
    public record GetBandeByIdQuery : IRequest<BandeResponseDto?>
    {
        /// <summary>
        /// ID de la bande à récupérer.
        /// </summary>
        public int Id { get; init; }
        
        /// <summary>
        /// Inclure les détails supplémentaires. Par défaut : false.
        /// </summary>
        public bool IncludeDetails { get; init; } = false;
    }
}