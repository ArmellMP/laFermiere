using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Achats.Interfaces;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAchatById
{
    public class GetAchatByIdQueryHandler : IRequestHandler<GetAchatByIdQuery, AchatResponseDto>
    {
        private readonly IAchatRepository _achatRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAchatByIdQueryHandler> _logger;

        public GetAchatByIdQueryHandler(
            IAchatRepository achatRepository,
            IMapper mapper,
            ILogger<GetAchatByIdQueryHandler> logger)
        {
            _achatRepository = achatRepository;
            _mapper = mapper;
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

            return _mapper.Map<AchatResponseDto>(achat);
        }
    }
}