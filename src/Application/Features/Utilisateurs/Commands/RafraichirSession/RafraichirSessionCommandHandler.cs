using Application.Common.Mappings;
using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Queries.AuthenticateUser;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Utilisateurs.Commands.RafraichirSession
{
    /// <summary>
    /// Renouvellement de session avec rotation et détection de réutilisation :
    ///   - token inconnu, expiré ou révoqué : refus
    ///   - token DÉJÀ utilisé (rejoué) : refus + toute la session est révoquée,
    ///     car seul un voleur peut présenter un token que le vrai client a déjà échangé
    ///   - compte supprimé ou désactivé : refus + session révoquée
    ///   - sinon : le token est marqué utilisé et un nouveau couple de jetons est émis
    ///     dans la même session, avec le rôle ACTUEL de l'utilisateur.
    /// Tous les refus ont le même message : le client n'apprend pas pourquoi.
    /// </summary>
    public class RafraichirSessionCommandHandler : IRequestHandler<RafraichirSessionCommand, AuthenticateUserResponse>
    {
        public const string MessageRefus = "Session expirée ou invalide. Veuillez vous reconnecter.";

        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly ISessionService _sessionService;
        private readonly ILogger<RafraichirSessionCommandHandler> _logger;

        public RafraichirSessionCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUtilisateurRepository utilisateurRepository,
            ISessionService sessionService,
            ILogger<RafraichirSessionCommandHandler> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _utilisateurRepository = utilisateurRepository;
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task<AuthenticateUserResponse> Handle(RafraichirSessionCommand request, CancellationToken cancellationToken)
        {
            var maintenant = DateTime.UtcNow;
            var stocke = await _refreshTokenRepository.GetByHashAsync(_sessionService.Empreinte(request.RefreshToken), cancellationToken);

            if (stocke == null || stocke.RevoqueLe != null || stocke.EstExpire(maintenant))
            {
                throw new UnauthorizedAccessException(MessageRefus);
            }

            if (stocke.UtiliseLe != null)
            {
                await RevoquerPourReutilisationAsync(stocke.FamilleId, stocke.UtilisateurId, maintenant, cancellationToken);
                throw new UnauthorizedAccessException(MessageRefus);
            }

            var utilisateur = await _utilisateurRepository.GetByIdAsync(stocke.UtilisateurId, cancellationToken);
            if (utilisateur == null || !utilisateur.EstActif)
            {
                await _refreshTokenRepository.RevoquerFamilleAsync(stocke.FamilleId, maintenant, cancellationToken);
                throw new UnauthorizedAccessException(MessageRefus);
            }

            // Atomique : si deux requêtes présentent le même token en même temps, une seule passe ;
            // l'autre est traitée comme une réutilisation.
            if (!await _refreshTokenRepository.MarquerUtiliseAsync(stocke.Id, maintenant, cancellationToken))
            {
                await RevoquerPourReutilisationAsync(stocke.FamilleId, stocke.UtilisateurId, maintenant, cancellationToken);
                throw new UnauthorizedAccessException(MessageRefus);
            }

            var jetons = await _sessionService.EmettreAsync(utilisateur, stocke.FamilleId, cancellationToken);

            return new AuthenticateUserResponse
            {
                Utilisateur = utilisateur.ToResponseDto(),
                Token = jetons.Token,
                Expiration = jetons.Expiration,
                RefreshToken = jetons.RefreshToken,
                RefreshExpiration = jetons.RefreshExpiration
            };
        }

        private async Task RevoquerPourReutilisationAsync(string familleId, int utilisateurId, DateTime maintenant, CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "Refresh token réutilisé pour l'utilisateur {UtilisateurId} : vol possible, session {FamilleId} révoquée",
                utilisateurId, familleId);
            await _refreshTokenRepository.RevoquerFamilleAsync(familleId, maintenant, cancellationToken);
        }
    }
}
