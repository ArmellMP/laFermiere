using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using MediatR;

namespace Application.Features.Utilisateurs.Commands.Deconnecter
{
    /// <summary>
    /// Révoque toute la session (le token présenté et ceux qui en sont issus).
    /// Un token inconnu ne provoque pas d'erreur : la déconnexion réussit toujours,
    /// sans indiquer si le token existait.
    /// Le jeton d'accès déjà émis reste valable jusqu'à son expiration (15 min au plus) :
    /// le client doit l'oublier.
    /// </summary>
    public class DeconnecterCommandHandler : IRequestHandler<DeconnecterCommand>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ISessionService _sessionService;

        public DeconnecterCommandHandler(IRefreshTokenRepository refreshTokenRepository, ISessionService sessionService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _sessionService = sessionService;
        }

        public async Task Handle(DeconnecterCommand request, CancellationToken cancellationToken)
        {
            var stocke = await _refreshTokenRepository.GetByHashAsync(_sessionService.Empreinte(request.RefreshToken), cancellationToken);
            if (stocke != null)
            {
                await _refreshTokenRepository.RevoquerFamilleAsync(stocke.FamilleId, DateTime.UtcNow, cancellationToken);
            }
        }
    }
}
