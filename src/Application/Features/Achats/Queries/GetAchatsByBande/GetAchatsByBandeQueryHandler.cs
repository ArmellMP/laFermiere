using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Achats.Queries.GetAchatsByBande
{
    public class GetAchatsByBandeQueryHandler : IRequestHandler<GetAchatsByBandeQuery, List<AchatResponseDto>>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<GetAchatsByBandeQueryHandler> _logger;

        public GetAchatsByBandeQueryHandler(
            IAchatRepository achatRepository,
            ILogger<GetAchatsByBandeQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _logger = logger;
        }

        public async Task<List<AchatResponseDto>> Handle(
            GetAchatsByBandeQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des achats de la bande {BandeId}", request.BandeId);

            var achats = await _achatRepository.GetByBandeIdAsync(request.BandeId, cancellationToken);
            
            return achats.ToResponseDtos();
        }
    }
}