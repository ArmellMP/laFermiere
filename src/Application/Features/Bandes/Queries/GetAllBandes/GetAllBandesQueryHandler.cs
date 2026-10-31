using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using MediatR;
using Application.Common.Mappings;

namespace Application.Features.Bandes.Queries.GetAllBandes
{
    /// <summary>
    /// Handler pour la query GetAllBandes.
    /// </summary>
    public class GetAllBandesQueryHandler 
        : IRequestHandler<GetAllBandesQuery, List<BandeResponseDto>>
    {
        private readonly IBandeRepository _bandeRepository;

        public GetAllBandesQueryHandler(
            IBandeRepository bandeRepository)
        {
            _bandeRepository = bandeRepository;
        }

        public async Task<List<BandeResponseDto>> Handle(
            GetAllBandesQuery request, 
            CancellationToken cancellationToken)
        {
            // Récupérer les bandes
            var bandes = await _bandeRepository.GetAllAsync(cancellationToken);
            
            // Convertir en DTOs
            var bandeDtos = bandes.ToResponseDtos();
            
            // Inclure les détails si demandé, request.IncludeDetails vient du record
            if (request.IncludeDetails)
            {
                foreach (var dto in bandeDtos)
                {
                    dto.IncludeDetails = true;
                }
            }
            
            return bandeDtos;
        }
    }
}