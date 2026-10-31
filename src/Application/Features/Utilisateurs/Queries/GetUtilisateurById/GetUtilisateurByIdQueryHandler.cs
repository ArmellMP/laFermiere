using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Dtos;

namespace Application.Features.Utilisateurs.Queries.GetUtilisateurById
{
    public class GetUtilisateurByIdQueryHandler : IRequestHandler<GetUtilisateurByIdQuery, UtilisateurResponseDto>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUtilisateurByIdQueryHandler> _logger;

        public GetUtilisateurByIdQueryHandler(
            IUtilisateurRepository utilisateurRepository,
            IMapper mapper,
            ILogger<GetUtilisateurByIdQueryHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _mapper = mapper;
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

            return _mapper.Map<UtilisateurResponseDto>(utilisateur);
        }
    }
}