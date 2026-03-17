using MediatR;

namespace Application.Features.Bandes.Commands.DeleteBande
{
    /// <summary>
    /// Command pour supprimer une bande.
    /// Retourne true si supprimé, false si non trouvé.
    /// </summary>
    public record DeleteBandeCommand : IRequest<bool>
    {
        /// <summary>
        /// ID de la bande à supprimer.
        /// </summary>
        public int Id { get; init; }
    }
}