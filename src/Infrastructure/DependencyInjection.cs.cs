using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.Repositories;
using Infrastructure.Repositories.BandeRepository; 
using Application.Features.Bandes.Interfaces;
using Application.Features.Stocks.Interfaces; 
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.Clients.Interfaces;
using Application.Features.Fournisseurs.Interfaces;
using Application.Features.Ventes.Interfaces;
using Application.Features.Achats.Interfaces;
using Application.Features.Utilisateurs.Interfaces;
using Application.Features.Parametres.Interfaces;
using Application.Features.Alertes.Interfaces;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // DapperContext (déjà existant)
            services.AddScoped<DapperContext>();
            
            // Repository Bande (déjà existant)
            services.AddScoped<IBandeRepository, BandeRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<ISuiviJournalierRepository, SuiviJournalierRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IFournisseurRepository, FournisseurRepository>();
            services.AddScoped<IVenteRepository, VenteRepository>();
            services.AddScoped<IAchatRepository, AchatRepository>();
            services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
            services.AddScoped<IParametreRepository, ParametreRepository>();
            services.AddScoped<IAlerteRepository, AlerteRepository>();
            
            return services;
        }
    }
}