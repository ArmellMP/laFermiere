using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Dtos;
using Application.Common.Mappings;

namespace Application.Features.Utilisateurs.Queries.GetUtilisateurById
{
    public class GetUtilisateurByIdQueryHandler : IRequestHandler<GetUtilisateurByIdQuery, UtilisateurResponseDto>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly ILogger<GetUtilisateurByIdQueryHandler> _logger;

        public GetUtilisateurByIdQueryHandler(
            IUtilisateurRepository utilisateurRepository,
            ILogger<GetUtilisateurByIdQueryHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _logger = logger;
        }

        public async Task<UtilisateurResponseDto> Handle(
            GetUtilisateurByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de l'utilisateur {UtilisateurId}", request.Id);

            var utilisateur = await _utilisateurRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (utilisateur == null)
            {
                throw new NotFoundException($"Utilisateur avec l'ID {request.Id} non trouvé.");
            }

            return utilisateur.ToResponseDto();
        }
    }
}