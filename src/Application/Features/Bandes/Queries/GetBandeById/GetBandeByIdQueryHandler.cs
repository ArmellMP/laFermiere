using Application.Common.Exceptions;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using MediatR;
using Application.Common.Mappings;

namespace Application.Features.Bandes.Queries.GetBandeById
{
    public class GetBandeByIdQueryHandler 
        : IRequestHandler<GetBandeByIdQuery, BandeResponseDto>
    {
        private readonly IBandeRepository _bandeRepository;

        public GetBandeByIdQueryHandler(
            IBandeRepository bandeRepository)
        {
            _bandeRepository = bandeRepository;
        }

        public async Task<BandeResponseDto> Handle(
            GetBandeByIdQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Récupérer la bande par ID
            var bande = await _bandeRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

            // 2. Si la bande n'existe pas : NotFoundException.
            //    L'ExceptionMiddleware la transforme en HTTP 404 avec ce message.
            //    (Avant : on retournait null, et le contrôleur renvoyait 204
            //    "succès sans contenu", ce qui faisait croire à une réussite.)
            if (bande == null)
                throw new NotFoundException($"Bande avec l'ID {request.Id} non trouvée.");
            
            // 3. Convertir en DTO
            var bandeDto = bande.ToResponseDto();
            
            // 4. Inclure les détails si demandé
            if (request.IncludeDetails)
                bandeDto.IncludeDetails = true;
            
            // 5. Retourner le DTO
            return bandeDto;
        }
    }
}