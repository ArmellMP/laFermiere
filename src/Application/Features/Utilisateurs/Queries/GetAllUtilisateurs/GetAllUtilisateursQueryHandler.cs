using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Utilisateurs.Dtos;
using Domain.Enums;

namespace Application.Features.Utilisateurs.Queries.GetAllUtilisateurs
{
    public class GetAllUtilisateursQueryHandler : IRequestHandler<GetAllUtilisateursQuery, List<UtilisateurResponseDto>>
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllUtilisateursQueryHandler> _logger;

        public GetAllUtilisateursQueryHandler(
            IUtilisateurRepository utilisateurRepository,
            IMapper mapper,
            ILogger<GetAllUtilisateursQueryHandler> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<UtilisateurResponseDto>> Handle(
            GetAllUtilisateursQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de la liste des utilisateurs");

            IEnumerable<Domain.Entities.Utilisateur> utilisateurs;

            if (!string.IsNullOrEmpty(request.Role))
            {
                if (Enum.TryParse<RoleUtilisateur>(request.Role, true, out var role))
                {
                    utilisateurs = await _utilisateurRepository.GetByRoleAsync(role, cancellationToken);
                }
                else
                {
                    return new List<UtilisateurResponseDto>();
                }
            }
            else if (request.OnlyActifs.HasValue && request.OnlyActifs.Value)
            {
                utilisateurs = await _utilisateurRepository.GetActifsAsync(cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                // Recherche par nom
                var allUsers = await _utilisateurRepository.GetAllAsync(cancellationToken);
                utilisateurs = allUsers.Where(u => u.Nom.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                utilisateurs = await _utilisateurRepository.GetAllAsync(cancellationToken);
            }

            return _mapper.Map<List<UtilisateurResponseDto>>(utilisateurs);
        }
    }
}