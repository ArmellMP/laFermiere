using Application.Features.Alertes.Dtos;
using MediatR;

namespace Application.Features.Alertes.Commands.GenererAlertes
{
    /// <summary>
    /// Passe en revue les stocks et les bandes actives et crée les alertes métier :
    /// stock faible, péremption proche ou dépassée, bande à clôturer.
    /// Lancée par l'endpoint POST /api/v1/Alertes/generer et par la tâche de fond périodique.
    /// </summary>
    public record GenererAlertesCommand : IRequest<GenerationAlertesResultDto>;
}
