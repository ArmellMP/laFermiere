using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Achats.Queries.GetAchatsByFournisseur
{
    public class GetAchatsByFournisseurQueryHandler : IRequestHandler<GetAchatsByFournisseurQuery, List<AchatResponseDto>>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<GetAchatsByFournisseurQueryHandler> _logger;

        public GetAchatsByFournisseurQueryHandler(
            IAchatRepository achatRepository,
            ILogger<GetAchatsByFournisseurQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _logger = logger;
        }

        public async Task<List<AchatResponseDto>> Handle(
            GetAchatsByFournisseurQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des achats du fournisseur {FournisseurId}", request.FournisseurId);

            var achats = await _achatRepository.GetByFournisseurIdAsync(request.FournisseurId, cancellationToken);
            
            return achats.ToResponseDtos();
        }
    }
}