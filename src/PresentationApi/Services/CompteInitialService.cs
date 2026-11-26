using Application.Features.Utilisateurs.Commands.SecuriserCompteInitial;
using MediatR;

namespace PresentationApi.Services
{
    /// <summary>
    /// Au démarrage : remplace le compte admin par défaut (admin@ferme.local / Admin123,
    /// mot de passe public) par CompteInitial:Email / CompteInitial:MotDePasse.
    ///
    ///   - Configuré : le remplacement est fait (une seule fois) ; une valeur invalide
    ///     empêche le démarrage, avec un message clair dans les journaux.
    ///   - Non configuré en Production : simple avertissement dans les journaux.
    ///   - Non configuré ailleurs (développement, tests) : rien.
    /// </summary>
    public class CompteInitialService : IHostedService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environnement;
        private readonly ILogger<CompteInitialService> _logger;

        public CompteInitialService(IServiceScopeFactory scopes, IConfiguration configuration, IHostEnvironment environnement, ILogger<CompteInitialService> logger)
        {
            _scopes = scopes;
            _configuration = configuration;
            _environnement = environnement;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var motDePasse = _configuration["CompteInitial:MotDePasse"];
            if (string.IsNullOrEmpty(motDePasse))
            {
                if (_environnement.IsProduction())
                    _logger.LogWarning(
                        "CompteInitial:MotDePasse n'est pas configuré : si la base vient de schema.sql, le compte {Email} au mot de passe public est toujours actif.",
                        SecuriserCompteInitialCommandHandler.EmailParDefaut);
                return;
            }

            using var scope = _scopes.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new SecuriserCompteInitialCommand(
                _configuration["CompteInitial:Email"] ?? string.Empty,
                motDePasse,
                _configuration["CompteInitial:Nom"]), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
