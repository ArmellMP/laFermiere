using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Parametres.Interfaces;
using Application.Features.Parametres.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Parametres.Queries.GetParametreByCle
{
    public class GetParametreByCleQueryHandler : IRequestHandler<GetParametreByCleQuery, ParametreResponseDto>
    {
        private readonly IParametreRepository _parametreRepository;
        private readonly ILogger<GetParametreByCleQueryHandler> _logger;

        public GetParametreByCleQueryHandler(
            IParametreRepository parametreRepository,
            ILogger<GetParametreByCleQueryHandler> logger)
        {
            _parametreRepository = parametreRepository;
            _logger = logger;
        }

        public async Task<ParametreResponseDto> Handle(
            GetParametreByCleQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du paramètre par clé : {Cle}", request.Cle);

            var parametre = await _parametreRepository.GetByCleAsync(request.Cle, cancellationToken);
            
            if (parametre == null)
            {
                throw new NotFoundException($"Paramètre avec la clé '{request.Cle}' non trouvé.");
            }

            return parametre.ToResponseDto();
        }
    }
}