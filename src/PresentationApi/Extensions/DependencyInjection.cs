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
            // ✅ 1. Services de la couche Application (MediatR, AutoMapper)
            services.AddApplication();
            
            // ✅ 2. Services de la couche Infrastructure (Repositories)
            services.AddInfrastructure(configuration);
            
            // ✅ 3. AutoMapper pour les Profiles de PresentationApi
            // Version simplifiée - évite l'ambiguïté
            services.AddAutoMapper(typeof(Program));
            
            // ✅ 4. Services spécifiques à l'API
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddHealthChecks();
            
            return services;
        }
    }
}