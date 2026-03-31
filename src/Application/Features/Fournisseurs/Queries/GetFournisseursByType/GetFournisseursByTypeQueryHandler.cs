using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Domain.Enums;

namespace Application.Features.Fournisseurs.Queries.GetFournisseursByType
{
    public class GetFournisseursByTypeQueryHandler : IRequestHandler<GetFournisseursByTypeQuery, List<FournisseurResponseDto>>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetFournisseursByTypeQueryHandler> _logger;

        public GetFournisseursByTypeQueryHandler(
            IFournisseurRepository fournisseurRepository,
            IMapper mapper,
            ILogger<GetFournisseursByTypeQueryHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<FournisseurResponseDto>> Handle(
            GetFournisseursByTypeQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des fournisseurs de type {TypeFournisseur}", request.TypeFournisseur);

            if (!Enum.TryParse<TypeFournisseur>(request.TypeFournisseur, true, out var typeFournisseur))
            {
                throw new BusinessRuleException(
                    $"Type de fournisseur invalide : {request.TypeFournisseur}. " +
                    "Valeurs acceptées : Poussiniere, Aliment, Medicament, Materiel, Services, Transport, Autre");
            }

            var fournisseurs = await _fournisseurRepository.GetByTypeAsync(typeFournisseur, cancellationToken);
            
            if (request.OnlyActifs)
            {
                fournisseurs = fournisseurs.Where(f => f.EstActif);
            }

            return _mapper.Map<List<FournisseurResponseDto>>(fournisseurs);
        }
    }
}