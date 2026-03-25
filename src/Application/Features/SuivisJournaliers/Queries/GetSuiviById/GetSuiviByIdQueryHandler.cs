using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.SuivisJournaliers.Dtos;
using Application.Features.Bandes.Interfaces;

namespace Application.Features.SuivisJournaliers.Queries.GetSuiviById
{
    public class GetSuiviByIdQueryHandler 
        : IRequestHandler<GetSuiviByIdQuery, SuiviResponseDto>
    {
        private readonly ISuiviJournalierRepository _suiviRepository;
        private readonly IBandeRepository _bandeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSuiviByIdQueryHandler> _logger;

        public GetSuiviByIdQueryHandler(
            ISuiviJournalierRepository suiviRepository,
            IBandeRepository bandeRepository,
            IMapper mapper,
            ILogger<GetSuiviByIdQueryHandler> logger)
        {
            _suiviRepository = suiviRepository;
            _bandeRepository = bandeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SuiviResponseDto> Handle(
            GetSuiviByIdQuery request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du suivi {SuiviId}", request.Id);

            // ============================================
            // 1. Récupérer le suivi
            // ============================================
            var suivi = await _suiviRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (suivi == null)
            {
                throw new NotFoundException($"Suivi avec l'ID {request.Id} non trouvé.");
            }

            // ============================================
            // 2. Récupérer la bande associée (pour DateArrivee)
            // ============================================
            var bande = await _bandeRepository.GetByIdAsync(suivi.BandeId, cancellationToken);
            
            // ============================================
            // 3. Mapper vers DTO
            // ============================================
            var suiviDto = _mapper.Map<SuiviResponseDto>(suivi);
            
            if (bande != null)
            {
                suiviDto.DateArriveeBande = bande.DateArrivee;
            }

            _logger.LogInformation("Suivi {SuiviId} trouvé", request.Id);

            return suiviDto;
        }
    }
}