// ============================================
// FICHIER: Application/Features/Utilisateurs/Queries/AuthenticateUser/AuthenticateUserQueryHandler.cs
// ============================================

using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Utilisateurs.Queries.AuthenticateUser
{
    /// <summary>
    /// Handler qui traite la requête d'authentification
    /// </summary>
    public class AuthenticateUserQueryHandler : IRequestHandler<AuthenticateUserQuery, AuthenticateUserResponse>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ISessionService _sessionService;
        private readonly ILogger<AuthenticateUserQueryHandler> _logger;

        public AuthenticateUserQueryHandler(
            IUtilisateurRepository utilisateurRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ISessionService sessionService,
            ILogger<AuthenticateUserQueryHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task<AuthenticateUserResponse> Handle(
            AuthenticateUserQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tentative d'authentification pour {Email}", request.Email);

            // 1. Vérifier les identifiants
            var utilisateur = await _utilisateurRepository.AuthenticateAsync(
                request.Email, 
                request.Password, 
                cancellationToken);
            
            if (utilisateur == null)
            {
                throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
            }

            if (!utilisateur.EstActif)
            {
                throw new UnauthorizedAccessException("Votre compte est désactivé. Veuillez contacter l'administrateur.");
            }

            // 2. Nouvelle session : jeton d'accès (15 min) + refresh token (7 jours).
            //    Ménage au passage : les refresh tokens expirés de cet utilisateur sont supprimés.
            await _refreshTokenRepository.SupprimerExpiresAsync(utilisateur.Id, DateTime.UtcNow, cancellationToken);
            var jetons = await _sessionService.EmettreAsync(utilisateur, familleId: null, cancellationToken);

            // 3. Mettre à jour la date de dernière connexion
            await _utilisateurRepository.UpdateDerniereConnexionAsync(
                utilisateur.Id, 
                DateTime.UtcNow, 
                cancellationToken);

            _logger.LogInformation("Utilisateur {UtilisateurId} authentifié avec succès", utilisateur.Id);

            // 4. Retourner la réponse
            return new AuthenticateUserResponse
            {
                Utilisateur = utilisateur.ToResponseDto(),
                Token = jetons.Token,
                Expiration = jetons.Expiration,
                RefreshToken = jetons.RefreshToken,
                RefreshExpiration = jetons.RefreshExpiration
            };
        }
    }
}