using Application.Features.Bandes.Dtos;
using MediatR;

namespace Application.Features.Bandes.Queries.GetBandeById
{
    /// <summary>
    /// Query pour récupérer une bande par son ID.
    /// Retourne toujours un DTO : si la bande n'existe pas, le handler lève
    /// NotFoundException (=> HTTP 404), comme pour toutes les autres entités.
    /// </summary>
    public record GetBandeByIdQuery : IRequest<BandeResponseDto>
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