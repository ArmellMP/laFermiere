using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Fournisseurs.Commands.CreateFournisseur
{
    public class CreateFournisseurCommandHandler : IRequestHandler<CreateFournisseurCommand, FournisseurResponseDto>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateFournisseurCommandHandler> _logger;

        public CreateFournisseurCommandHandler(
            IFournisseurRepository fournisseurRepository,
            IMapper mapper,
            ILogger<CreateFournisseurCommandHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FournisseurResponseDto> Handle(
            CreateFournisseurCommand request,
            CancellationToken cancellationToken)
        {
            var createDto = request.CreateFournisseurDto;

            _logger.LogInformation("Création d'un nouveau fournisseur : {Nom}", createDto.Nom);

            // Conversion du type de fournisseur (string → enum)
            if (!Enum.TryParse<TypeFournisseur>(createDto.TypeFournisseur, true, out var typeFournisseur))
            {
                throw new BusinessRuleException($"Type de fournisseur invalide : {createDto.TypeFournisseur}");
            }

            // Création de l'entité
            var fournisseur = new Fournisseur
            {
                Nom = createDto.Nom,
                TypeFournisseur = typeFournisseur,
                Telephone = createDto.Telephone,
                Email = createDto.Email,
                Adresse = createDto.Adresse,
                ContactPersonne = createDto.ContactPersonne,
                Notes = createDto.Notes,
                EstActif = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Sauvegarde
            var createdFournisseur = await _fournisseurRepository.AddAsync(fournisseur, cancellationToken);

            _logger.LogInformation("Fournisseur créé avec succès : ID {FournisseurId}", createdFournisseur.Id);

            // Retour de la réponse
            return _mapper.Map<FournisseurResponseDto>(createdFournisseur);
        }
    }
}