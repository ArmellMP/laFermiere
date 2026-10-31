// Application/Features/Utilisateurs/Commands/CreateUtilisateur/CreateUtilisateurCommandHandler.cs
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Utilisateurs.Commands.CreateUtilisateur
{
    public class CreateUtilisateurCommandHandler : IRequestHandler<CreateUtilisateurCommand, UtilisateurResponseDto>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateUtilisateurCommandHandler> _logger;

        public CreateUtilisateurCommandHandler(
            IUtilisateurRepository utilisateurRepository,
            IMapper mapper,
            ILogger<CreateUtilisateurCommandHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UtilisateurResponseDto> Handle(
            CreateUtilisateurCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateUtilisateurDto;

            _logger.LogInformation("Création d'un nouvel utilisateur : {Email}", createDto.Email);

            // ============================================
            // 1. Vérifier que l'email n'existe pas déjà
            // ============================================
            // ✅ CORRECTION : Ajouter null pour excludeId
            var emailExists = await _utilisateurRepository.EmailExistsAsync(createDto.Email, null, cancellationToken);
            if (emailExists)
            {
                throw new BusinessRuleException($"Un utilisateur avec l'email '{createDto.Email}' existe déjà.");
            }

            // ============================================
            // 2. Conversion du rôle (string → enum)
            // ============================================
            if (!Enum.TryParse<RoleUtilisateur>(createDto.Role, true, out var role))
            {
                throw new BusinessRuleException($"Rôle invalide : {createDto.Role}");
            }

            // ============================================
            // 3. Hashage du mot de passe
            // ============================================
            var passwordHash = PasswordHasher.HashPassword(createDto.Password);

            // ============================================
            // 4. Création de l'entité
            // ============================================
            var utilisateur = new Utilisateur
            {
                Nom = createDto.Nom,
                Email = createDto.Email,
                PasswordHash = passwordHash,
                Role = role,
                EstActif = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // ============================================
            // 5. Sauvegarde
            // ============================================
            var createdUtilisateur = await _utilisateurRepository.AddAsync(utilisateur, cancellationToken);

            _logger.LogInformation("Utilisateur créé avec succès : ID {UtilisateurId}, Email {Email}",
                createdUtilisateur.Id, createdUtilisateur.Email);

            // ============================================
            // 6. Retour de la réponse
            // ============================================
            return _mapper.Map<UtilisateurResponseDto>(createdUtilisateur);
        }
    }
}