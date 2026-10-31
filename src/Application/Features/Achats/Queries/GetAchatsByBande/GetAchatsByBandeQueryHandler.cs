using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAchatsByBande
{
    public class GetAchatsByBandeQueryHandler : IRequestHandler<GetAchatsByBandeQuery, List<AchatResponseDto>>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAchatsByBandeQueryHandler> _logger;

        public GetAchatsByBandeQueryHandler(
            IAchatRepository achatRepository,
            IMapper mapper,
            ILogger<GetAchatsByBandeQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<AchatResponseDto>> Handle(
            GetAchatsByBandeQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des achats de la bande {BandeId}", request.BandeId);

            var achats = await _achatRepository.GetByBandeIdAsync(request.BandeId, cancellationToken);
            
            return _mapper.Map<List<AchatResponseDto>>(achats);
        }
    }
}