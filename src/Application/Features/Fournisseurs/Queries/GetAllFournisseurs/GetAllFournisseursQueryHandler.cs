using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Fournisseurs.Queries.GetAllFournisseurs
{
    public class GetAllFournisseursQueryHandler : IRequestHandler<GetAllFournisseursQuery, List<FournisseurResponseDto>>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly ILogger<GetAllFournisseursQueryHandler> _logger;

        public GetAllFournisseursQueryHandler(
            IFournisseurRepository fournisseurRepository,
            ILogger<GetAllFournisseursQueryHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _logger = logger;
        }

        public async Task<List<FournisseurResponseDto>> Handle(
            GetAllFournisseursQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des fournisseurs");

            IEnumerable<Domain.Entities.Fournisseur> fournisseurs;

            if (!string.IsNullOrEmpty(request.TypeFournisseur))
            {
                if (Enum.TryParse<TypeFournisseur>(request.TypeFournisseur, true, out var type))
                {
                    fournisseurs = await _fournisseurRepository.GetByTypeAsync(type, cancellationToken);
                }
                else
                {
                    return new List<FournisseurResponseDto>();
                }
            }
            else if (request.OnlyActifs.HasValue && request.OnlyActifs.Value)
            {
                fournisseurs = await _fournisseurRepository.GetActifsAsync(cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                fournisseurs = await _fournisseurRepository.GetByNomAsync(request.SearchTerm, cancellationToken);
            }
            else
            {
                fournisseurs = await _fournisseurRepository.GetAllAsync(cancellationToken);
            }

            return fournisseurs.ToResponseDtos();
        }
    }
}