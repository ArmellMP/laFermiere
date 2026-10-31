using MediatR;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;
using Application.Features.Parametres.Interfaces;

namespace Application.Features.Parametres.Commands.DeleteParametre
{
    public class DeleteParametreCommandHandler : IRequestHandler<DeleteParametreCommand, Unit>
    {
        private readonly IParametreRepository _parametreRepository;
        private readonly ILogger<DeleteParametreCommandHandler> _logger;

        public DeleteParametreCommandHandler(
            IParametreRepository parametreRepository,
            ILogger<DeleteParametreCommandHandler> logger)
        {
            _parametreRepository = parametreRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteParametreCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suppression du paramètre {ParametreId}", request.Id);

            // 1. Vérifier que le paramètre existe
            var exists = await _parametreRepository.ExistsAsync(request.Id, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException($"Paramètre avec l'ID {request.Id} non trouvé.");
            }

            // 2. Supprimer le paramètre
            await _parametreRepository.DeleteAsync(request.Id, cancellationToken);

            _logger.LogInformation("Paramètre {ParametreId} supprimé avec succès", request.Id);

            return Unit.Value;
        }
    }
}