using Application.Features.Alertes.Commands.GenererAlertes;
using MediatR;

namespace PresentationApi.Services
{
    /// <summary>
    /// Tâche de fond : génère les alertes métier à intervalle régulier, sans action d'un utilisateur.
    ///
    /// Réglage (appsettings.json) : "Alertes": { "IntervalleGenerationHeures": 6 }
    ///   0 ou valeur négative = tâche désactivée (cas des tests d'intégration).
    ///
    /// Première génération peu après le démarrage de l'API, puis toutes les N heures.
    /// Une erreur (base indisponible...) est journalisée sans arrêter l'API : on réessaie au tour suivant.
    /// </summary>
    public class GenerationAlertesBackgroundService : BackgroundService
    {
        public const string CleIntervalle = "Alertes:IntervalleGenerationHeures";

        private static readonly TimeSpan DelaiAuDemarrage = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GenerationAlertesBackgroundService> _logger;
        private readonly double _intervalleHeures;

        public GenerationAlertesBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<GenerationAlertesBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _intervalleHeures = configuration.GetValue(CleIntervalle, 6.0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_intervalleHeures <= 0)
            {
                _logger.LogInformation("Génération automatique des alertes désactivée ({Cle} = {Valeur})",
                    CleIntervalle, _intervalleHeures);
                return;
            }

            try
            {
                await Task.Delay(DelaiAuDemarrage, stoppingToken);

                using var minuterie = new PeriodicTimer(TimeSpan.FromHours(_intervalleHeures));
                do
                {
                    await GenererAsync(stoppingToken);
                }
                while (await minuterie.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Arrêt normal de l'API
            }
        }

        private async Task GenererAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Les repositories sont "scoped" : un scope par génération,
                // comme pour une requête HTTP.
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new GenererAlertesCommand(), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Échec de la génération automatique des alertes ; nouvel essai dans {Heures} h",
                    _intervalleHeures);
            }
        }
    }
}
