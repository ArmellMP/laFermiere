using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Parametres.Interfaces;
using Application.Features.Parametres.Dtos;
using Domain.Enums;
using Application.Common.Mappings;

namespace Application.Features.Parametres.Queries.GetAllParametres
{
    public class GetAllParametresQueryHandler : IRequestHandler<GetAllParametresQuery, List<ParametreResponseDto>>
    {
        private readonly IParametreRepository _parametreRepository;
        private readonly ILogger<GetAllParametresQueryHandler> _logger;

        public GetAllParametresQueryHandler(
            IParametreRepository parametreRepository,
            ILogger<GetAllParametresQueryHandler> logger)
        {
            _parametreRepository = parametreRepository;
            _logger = logger;
        }

        public async Task<List<ParametreResponseDto>> Handle(
            GetAllParametresQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des paramètres");

            IEnumerable<Domain.Entities.Parametre> parametres;

            if (!string.IsNullOrEmpty(request.Categorie))
            {
                if (Enum.TryParse<CategorieParametre>(request.Categorie, true, out var categorie))
                {
                    parametres = await _parametreRepository.GetByCategorieAsync(categorie, cancellationToken);
                }
                else
                {
                    return new List<ParametreResponseDto>();
                }
            }
            else if (request.OnlyModifiables.HasValue && request.OnlyModifiables.Value)
            {
                parametres = await _parametreRepository.GetModifiablesAsync(cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var allParams = await _parametreRepository.GetAllAsync(cancellationToken);
                parametres = allParams.Where(p => 
                    p.Cle.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                parametres = await _parametreRepository.GetAllAsync(cancellationToken);
            }

            return parametres.ToResponseDtos();
        }
    }
}