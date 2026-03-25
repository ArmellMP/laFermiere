using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.DbContext.DapperContext;
using Infrastructure.Repositories;
using Infrastructure.Repositories.BandeRepository; 
using Application.Features.Bandes.Interfaces;
using Application.Features.Stocks.Interfaces; 
using Application.Features.SuivisJournaliers.Interfaces;
using Application.Features.Clients.Interfaces;

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
            
            // Autres repositories (à décommenter plus tard)
            // services.AddScoped<IVenteRepository, VenteRepository>();
            // services.AddScoped<IAchatRepository, AchatRepository>();
            // services.AddScoped<IFournisseurRepository, FournisseurRepository>();
            
            return services;
        }
    }
}