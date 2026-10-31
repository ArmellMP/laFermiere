using MediatR;

namespace Application.Features.Bandes.Commands.DeleteBande
{
    /// <summary>
    /// Command pour supprimer une bande.
    /// Unit = "aucune valeur de retour" (équivalent de void pour MediatR).
    /// Si la bande n'existe pas, le handler lève NotFoundException (=> HTTP 404).
    /// </summary>
    public record DeleteBandeCommand : IRequest<Unit>
    {
        /// <summary>
        /// ID de la bande à supprimer.
        /// </summary>
        public int Id { get; init; }
    }
}