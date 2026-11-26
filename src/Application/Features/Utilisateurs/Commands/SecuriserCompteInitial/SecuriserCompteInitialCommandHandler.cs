using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Utilisateurs.Commands.SecuriserCompteInitial
{
    public class SecuriserCompteInitialCommandHandler : IRequestHandler<SecuriserCompteInitialCommand, ResultatCompteInitial>
    {
        /// <summary>Compte créé par database/schema.sql sur une base neuve.</summary>
        public const string EmailParDefaut = "admin@ferme.local";
        public const string MotDePasseParDefaut = "Admin123";

        private readonly IUtilisateurRepository _utilisateurs;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ILogger<SecuriserCompteInitialCommandHandler> _logger;

        public SecuriserCompteInitialCommandHandler(
            IUtilisateurRepository utilisateurs,
            IRefreshTokenRepository refreshTokens,
            ILogger<SecuriserCompteInitialCommandHandler> logger)
        {
            _utilisateurs = utilisateurs;
            _refreshTokens = refreshTokens;
            _logger = logger;
        }

        public async Task<ResultatCompteInitial> Handle(SecuriserCompteInitialCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();
            VerifierMotDePasse(request.MotDePasse);
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new BusinessRuleException("CompteInitial:Email doit être une adresse e-mail valide.");

            var admins = await _utilisateurs.GetByRoleAsync(RoleUtilisateur.Admin, cancellationToken);
            var parDefaut = admins.FirstOrDefault(u =>
                string.Equals(u.Email, EmailParDefaut, StringComparison.OrdinalIgnoreCase)
                && PasswordHasher.VerifyPassword(MotDePasseParDefaut, u.PasswordHash));

            if (parDefaut == null)
            {
                _logger.LogInformation("Compte administrateur initial : déjà sécurisé, rien à faire");
                return ResultatCompteInitial.DejaSecurise;
            }

            if (!string.Equals(email, parDefaut.Email, StringComparison.OrdinalIgnoreCase)
                && await _utilisateurs.EmailExistsAsync(email, parDefaut.Id, cancellationToken))
            {
                throw new ConflictException($"CompteInitial:Email '{email}' est déjà utilisé par un autre compte.");
            }

            parDefaut.Email = email;
            if (!string.IsNullOrWhiteSpace(request.Nom)) parDefaut.Nom = request.Nom.Trim();
            parDefaut.PasswordHash = PasswordHasher.HashPassword(request.MotDePasse);
            parDefaut.EstActif = true;
            parDefaut.UpdatedAt = DateTime.UtcNow;

            await _utilisateurs.UpdateAsync(parDefaut, cancellationToken);
            await _refreshTokens.RevoquerToutPourUtilisateurAsync(parDefaut.Id, DateTime.UtcNow, cancellationToken);

            _logger.LogWarning("Compte administrateur par défaut remplacé : connexion désormais avec {Email}", email);
            return ResultatCompteInitial.Securise;
        }

        /// <summary>Au moins aussi strict que les règles habituelles, et différent de l'ancien mot de passe public.</summary>
        private static void VerifierMotDePasse(string motDePasse)
        {
            var valide = motDePasse.Length is >= 8 and <= 100
                && motDePasse.Any(char.IsUpper)
                && motDePasse.Any(char.IsLower)
                && motDePasse.Any(char.IsDigit)
                && motDePasse != MotDePasseParDefaut;
            if (!valide)
                throw new BusinessRuleException(
                    "CompteInitial:MotDePasse doit contenir au moins 8 caractères, une majuscule, une minuscule et un chiffre (et ne pas être Admin123).");
        }
    }
}
