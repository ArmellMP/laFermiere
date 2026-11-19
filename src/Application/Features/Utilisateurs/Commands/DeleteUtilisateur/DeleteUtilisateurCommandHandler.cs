using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Regles;
using Domain.Enums;

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
            var utilisateur = await _utilisateurRepository.GetByIdAsync(request.Id, cancellationToken);
            if (utilisateur == null)
            {
                throw new NotFoundException($"Utilisateur avec l'ID {request.Id} non trouvé.");
            }

            // 2. Le dernier administrateur actif ne peut pas être supprimé
            if (utilisateur.Role == RoleUtilisateur.Admin && utilisateur.EstActif)
            {
                await RegleAdministrateurs.VerifierQuUnAutreAdminResteAsync(_utilisateurRepository, utilisateur.Id, cancellationToken);
            }

            // 3. Supprimer l'utilisateur. S'il a déjà saisi des données (bandes, suivis…), la base
            //    refuse (clé étrangère) : ExceptionMiddleware répond 409 en conseillant de le désactiver.
            await _utilisateurRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Utilisateur {UtilisateurId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}