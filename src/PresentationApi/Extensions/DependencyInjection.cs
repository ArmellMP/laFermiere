using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application;
using Infrastructure;

namespace PresentationApi.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // ✅ 1. Services de la couche Application (MediatR, FluentValidation)
            services.AddApplication();

            // ✅ 2. Services de la couche Infrastructure (Repositories)
            services.AddInfrastructure(configuration);

            // (Le mapping Requests/Responses <-> Commands/DTOs se fait par des méthodes
            //  d'extension statiques dans PresentationApi/Mappings : rien à enregistrer.)

            // ✅ 3. Services spécifiques à l'API
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddHealthChecks();
            
            return services;
        }
    }
}