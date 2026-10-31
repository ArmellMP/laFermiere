using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Achats.Queries.GetAchatById
{
    public class GetAchatByIdQueryHandler : IRequestHandler<GetAchatByIdQuery, AchatResponseDto>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly ILogger<GetAchatByIdQueryHandler> _logger;

        public GetAchatByIdQueryHandler(
            IAchatRepository achatRepository,
            ILogger<GetAchatByIdQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _logger = logger;
        }

        public async Task<AchatResponseDto> Handle(
            GetAchatByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de l'achat {AchatId}", request.Id);

            var achat = await _achatRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (achat == null)
            {
                throw new NotFoundException($"Achat avec l'ID {request.Id} non trouvé.");
            }

            return achat.ToResponseDto();
        }
    }
}