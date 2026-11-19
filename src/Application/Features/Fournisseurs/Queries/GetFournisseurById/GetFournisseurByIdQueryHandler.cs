using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Fournisseurs.Dtos;
using Application.Features.Achats.Interfaces;
using Application.Common.Mappings;

namespace Application.Features.Fournisseurs.Queries.GetFournisseurById
{
    public class GetFournisseurByIdQueryHandler : IRequestHandler<GetFournisseurByIdQuery, FournisseurResponseDto>
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<GetFournisseurByIdQueryHandler> _logger;

        public GetFournisseurByIdQueryHandler(
            IFournisseurRepository fournisseurRepository,
            IAchatRepository achatRepository,
            ILogger<GetFournisseurByIdQueryHandler> logger)
        {
            _fournisseurRepository = fournisseurRepository;
            _achatRepository = achatRepository;
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

            // Statistiques d'achats auprès de ce fournisseur (une seule requête d'agrégat)
            var (nombreAchats, montantTotal) = await _achatRepository.GetStatsByFournisseurAsync(request.Id, cancellationToken);
            dto.NombreAchats = nombreAchats;
            dto.MontantTotalAchats = montantTotal;

            return dto;
        }
    }
}