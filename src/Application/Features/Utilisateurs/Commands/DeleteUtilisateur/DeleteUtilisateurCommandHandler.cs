using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Utilisateurs.Interfaces;

namespace Application.Features.Utilisateurs.Commands.DeleteUtilisateur
{
    public class DeleteUtilisateurCommandHandler : IRequestHandler<DeleteUtilisateurCommand, Unit>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly ILogger<DeleteUtilisateurCommandHandler> _logger;

        public DeleteUtilisateurCommandHandler(
            IUtilisateurRepository utilisateurRepository,
            ILogger<DeleteUtilisateurCommandHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteUtilisateurCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression de l'utilisateur {UtilisateurId}", request.Id);

            // 1. Vérifier que l'utilisateur existe
            var exists = await _utilisateurRepository.ExistsAsync(request.Id, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException($"Utilisateur avec l'ID {request.Id} non trouvé.");
            }

            // 2. Supprimer l'utilisateur
            await _utilisateurRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Utilisateur {UtilisateurId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}