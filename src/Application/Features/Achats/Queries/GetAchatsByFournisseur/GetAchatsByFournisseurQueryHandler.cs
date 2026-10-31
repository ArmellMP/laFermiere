using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAchatsByFournisseur
{
    public class GetAchatsByFournisseurQueryHandler : IRequestHandler<GetAchatsByFournisseurQuery, List<AchatResponseDto>>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAchatsByFournisseurQueryHandler> _logger;

        public GetAchatsByFournisseurQueryHandler(
            IAchatRepository achatRepository,
            IMapper mapper,
            ILogger<GetAchatsByFournisseurQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<AchatResponseDto>> Handle(
            GetAchatsByFournisseurQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des achats du fournisseur {FournisseurId}", request.FournisseurId);

            var achats = await _achatRepository.GetByFournisseurIdAsync(request.FournisseurId, cancellationToken);
            
            return _mapper.Map<List<AchatResponseDto>>(achats);
        }
    }
}