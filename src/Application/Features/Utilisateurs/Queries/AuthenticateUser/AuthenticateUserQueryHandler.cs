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
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthenticateUserQueryHandler> _logger;

        public AuthenticateUserQueryHandler(
            IUtilisateurRepository utilisateurRepository,
            IJwtService jwtService,
            ILogger<AuthenticateUserQueryHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _jwtService = jwtService;
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

            // 2. Générer le token JWT
            var token = _jwtService.GenerateToken(
                utilisateur.Id, 
                utilisateur.Email, 
                utilisateur.Role.ToString());

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
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60) // À configurer
            };
        }
    }
}