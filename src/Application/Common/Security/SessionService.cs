using System.Security.Cryptography;
using System.Text;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Common.Security
{
    /// <summary>Jetons remis au client après une connexion ou un renouvellement.</summary>
    public record JetonsSession(string Token, DateTime Expiration, string RefreshToken, DateTime RefreshExpiration);

    public interface ISessionService
    {
        /// <summary>
        /// Émet un jeton d'accès (JWT) et un nouveau refresh token.
        /// familleId null : nouvelle session (connexion) ; sinon : renouvellement de cette session.
        /// </summary>
        Task<JetonsSession> EmettreAsync(Utilisateur utilisateur, string? familleId, CancellationToken cancellationToken);

        /// <summary>Empreinte SHA-256 (hexadécimal) d'un refresh token : seule valeur stockée en base.</summary>
        string Empreinte(string refreshToken);
    }

    /// <summary>
    /// Sessions de connexion : jeton d'accès court (15 min) + refresh token (7 jours).
    ///
    /// Le refresh token est une valeur aléatoire de 64 octets (aucune information dedans),
    /// et la base n'en garde que l'empreinte SHA-256. Sa durée est glissante : chaque
    /// renouvellement repart pour JwtSettings:RefreshTokenJours (7 par défaut). Un utilisateur
    /// qui revient au moins une fois par semaine reste donc connecté.
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public SessionService(IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        private TimeSpan DureeRefreshToken =>
            TimeSpan.FromDays(int.Parse(_configuration["JwtSettings:RefreshTokenJours"] ?? "7"));

        public async Task<JetonsSession> EmettreAsync(Utilisateur utilisateur, string? familleId, CancellationToken cancellationToken)
        {
            var maintenant = DateTime.UtcNow;

            var token = _jwtService.GenerateToken(utilisateur.Id, utilisateur.Email, utilisateur.Role.ToString());
            var expiration = maintenant.Add(_jwtService.DureeValidite);

            // 64 octets aléatoires, encodés en Base64 "URL" (sans + / =) : 86 caractères
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var refreshExpiration = maintenant.Add(DureeRefreshToken);

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UtilisateurId = utilisateur.Id,
                TokenHash = Empreinte(refreshToken),
                FamilleId = familleId ?? Guid.NewGuid().ToString(),
                ExpireLe = refreshExpiration,
                CreatedAt = maintenant
            }, cancellationToken);

            return new JetonsSession(token, expiration, refreshToken, refreshExpiration);
        }

        public string Empreinte(string refreshToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))).ToLowerInvariant();
    }
}
