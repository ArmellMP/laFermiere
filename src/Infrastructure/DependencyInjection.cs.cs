// Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.Repositories;
using Infrastructure.Repositories.BandeRepository; // existant
using Application.Features.Bandes.Interfaces;
using Application.Features.Stocks.Interfaces; // AJOUT

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
            
            // Autres repositories (à décommenter plus tard)
            // services.AddScoped<ISuiviJournalierRepository, SuiviJournalierRepository>();
            // services.AddScoped<IVenteRepository, VenteRepository>();
            // services.AddScoped<IAchatRepository, AchatRepository>();
            // services.AddScoped<IClientRepository, ClientRepository>();
            // services.AddScoped<IFournisseurRepository, FournisseurRepository>();
            
            return services;
        }
    }
}