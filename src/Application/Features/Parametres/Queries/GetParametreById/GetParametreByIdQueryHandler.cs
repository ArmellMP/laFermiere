using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Parametres.Interfaces;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Queries.GetParametreById
{
    public class GetParametreByIdQueryHandler : IRequestHandler<GetParametreByIdQuery, ParametreResponseDto>
    {
        private readonly IParametreRepository _parametreRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetParametreByIdQueryHandler> _logger;

        public GetParametreByIdQueryHandler(
            IParametreRepository parametreRepository,
            IMapper mapper,
            ILogger<GetParametreByIdQueryHandler> logger)
        {
            _parametreRepository = parametreRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ParametreResponseDto> Handle(
            GetParametreByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du paramètre {ParametreId}", request.Id);

            var parametre = await _parametreRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (parametre == null)
            {
                throw new NotFoundException($"Paramètre avec l'ID {request.Id} non trouvé.");
            }

            return _mapper.Map<ParametreResponseDto>(parametre);
        }
    }
}