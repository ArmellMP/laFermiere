using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Fournisseurs.Queries.GetFournisseurById
{
    public class GetFournisseurByIdQueryHandler : IRequestHandler<GetFournisseurByIdQuery, FournisseurResponseDto>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly ILogger<GetFournisseurByIdQueryHandler> _logger;

        public GetFournisseurByIdQueryHandler(
            IFournisseurRepository fournisseurRepository,
            ILogger<GetFournisseurByIdQueryHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _logger = logger;
        }

        public async Task<FournisseurResponseDto> Handle(
            GetFournisseurByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du fournisseur {FournisseurId}", request.Id);

            var fournisseur = await _fournisseurRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (fournisseur == null)
            {
                throw new NotFoundException($"Fournisseur avec l'ID {request.Id} non trouvé.");
            }

            var dto = fournisseur.ToResponseDto();

            if (request.IncludeAchats)
            {
                // TODO: Récupérer les achats quand l'entité sera créée
                // dto.NombreAchats = await _achatRepository.GetCountByFournisseurIdAsync(request.Id);
                // dto.MontantTotalAchats = await _achatRepository.GetTotalByFournisseurIdAsync(request.Id);
            }

            return dto;
        }
    }
}