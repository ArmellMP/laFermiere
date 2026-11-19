using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Dtos;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Utilisateurs.Commands.UpdateUtilisateur
{
    public class UpdateUtilisateurCommandHandler : IRequestHandler<UpdateUtilisateurCommand, UtilisateurResponseDto>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<UpdateUtilisateurCommandHandler> _logger;

        public UpdateUtilisateurCommandHandler(
            IUtilisateurRepository utilisateurRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<UpdateUtilisateurCommandHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<UtilisateurResponseDto> Handle(
            UpdateUtilisateurCommand request,
            CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateUtilisateurDto;

            _logger.LogInformation("Mise à jour de l'utilisateur {UtilisateurId}", updateDto.Id);

            // 1. Récupérer l'utilisateur existant
            var existingUtilisateur = await _utilisateurRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingUtilisateur == null)
            {
                throw new NotFoundException($"Utilisateur avec l'ID {updateDto.Id} non trouvé.");
            }

            // Valeurs avant modification : servent à décider s'il faut fermer ses sessions (étape 4)
            var ancienRole = existingUtilisateur.Role;
            var etaitActif = existingUtilisateur.EstActif;

            // 2. Vérifier l'unicité de l'email (si changement)
            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingUtilisateur.Email)
            {
                var emailExists = await _utilisateurRepository.EmailExistsAsync(updateDto.Email, updateDto.Id, cancellationToken);
                if (emailExists)
                {
                    throw new ConflictException($"Un utilisateur avec l'email '{updateDto.Email}' existe déjà.");
                }
                existingUtilisateur.Email = updateDto.Email;
            }

            // 3. Mise à jour des propriétés
            if (!string.IsNullOrEmpty(updateDto.Nom))
                existingUtilisateur.Nom = updateDto.Nom;

            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                existingUtilisateur.PasswordHash = PasswordHasher.HashPassword(updateDto.Password);
            }

            if (!string.IsNullOrEmpty(updateDto.Role))
            {
                if (Enum.TryParse<RoleUtilisateur>(updateDto.Role, true, out var role))
                {
                    existingUtilisateur.Role = role;
                }
            }

            if (updateDto.EstActif.HasValue)
                existingUtilisateur.EstActif = updateDto.EstActif.Value;

            existingUtilisateur.UpdatedAt = DateTime.UtcNow;

            // 4. Sauvegarde
            await _utilisateurRepository.UpdateAsync(existingUtilisateur, cancellationToken);

            // Mot de passe, rôle changé ou compte désactivé : toutes ses sessions sont fermées.
            // Les refresh tokens sont révoqués ; le jeton d'accès en cours expire en 15 min au plus.
            var motDePasseChange = !string.IsNullOrEmpty(updateDto.Password);
            var roleChange = existingUtilisateur.Role != ancienRole;
            var desactive = etaitActif && !existingUtilisateur.EstActif;
            if (motDePasseChange || roleChange || desactive)
            {
                await _refreshTokenRepository.RevoquerToutPourUtilisateurAsync(existingUtilisateur.Id, DateTime.UtcNow, cancellationToken);
                _logger.LogInformation("Sessions de l'utilisateur {UtilisateurId} révoquées (mot de passe, rôle ou désactivation)", existingUtilisateur.Id);
            }

            _logger.LogInformation("Utilisateur {UtilisateurId} mis à jour avec succès", updateDto.Id);

            // 5. Retour de la réponse
            return existingUtilisateur.ToResponseDto();
        }
    }
}