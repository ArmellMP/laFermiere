using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Bandes.Queries.GetBandesByStatut
{
    /// <summary>
    /// Handler pour récupérer les bandes par statut.
    /// </summary>
    public class GetBandesByStatutQueryHandler 
        : IRequestHandler<GetBandesByStatutQuery, List<BandeResponseDto>>
    {
        private readonly IBandeRepository _bandeRepository;
        private readonly IMapper _mapper;

        public GetBandesByStatutQueryHandler(
            IBandeRepository bandeRepository,
            IMapper mapper)
        {
            _bandeRepository = bandeRepository;
            _mapper = mapper;
        }

        public async Task<List<BandeResponseDto>> Handle(
            GetBandesByStatutQuery request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer les bandes par statut
            var bandes = await _bandeRepository.GetByStatutAsync(
                request.Statut, 
                cancellationToken);
            
            // 2. Convertir en DTOs
            var bandeDtos = _mapper.Map<List<BandeResponseDto>>(bandes);
            
            // 3. Inclure les détails si demandé
            if (request.IncludeDetails)
            {
                foreach (var dto in bandeDtos)
                {
                    dto.IncludeDetails = true;
                }
            }
            
            // 4. Retourner les DTOs
            return bandeDtos;
        }
    }
}