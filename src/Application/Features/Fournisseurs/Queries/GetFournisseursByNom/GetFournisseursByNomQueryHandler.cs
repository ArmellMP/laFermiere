using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Fournisseurs.Queries.GetFournisseursByNom
{
    public class GetFournisseursByNomQueryHandler : IRequestHandler<GetFournisseursByNomQuery, List<FournisseurResponseDto>>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly ILogger<GetFournisseursByNomQueryHandler> _logger;

        public GetFournisseursByNomQueryHandler(
            IFournisseurRepository fournisseurRepository,
            ILogger<GetFournisseursByNomQueryHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _logger = logger;
        }

        public async Task<List<FournisseurResponseDto>> Handle(
            GetFournisseursByNomQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recherche de fournisseurs avec le terme '{SearchTerm}'", request.SearchTerm);

            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return new List<FournisseurResponseDto>();
            }

            var fournisseurs = await _fournisseurRepository.GetByNomAsync(request.SearchTerm, cancellationToken);
            var limitedFournisseurs = fournisseurs.Take(request.Limit);

            return limitedFournisseurs.ToResponseDtos();
        }
    }
}
