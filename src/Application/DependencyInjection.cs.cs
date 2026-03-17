// Application/DependencyInjection.cs

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;// Pour Assembly.GetExecutingAssembly()

using FluentValidation; // Pour la validation automatique

namespace Application
{
    /// <summary>
    /// Configuration des services pour la couche Application.
    /// Responsable de l'enregistrement de tous les services liés à :
    /// - MediatR (CQRS : Commands/Queries/Handlers)
    /// - AutoMapper (Mapping entre DTOs et Entities)
    /// - FluentValidation (Validation des DTOs)
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Ajoute tous les services de la couche Application.
        /// Cette méthode sera appelée depuis PresentationAPI.
        /// </summary>
        /// <param name="services">Collection des services</param>
        /// <returns>La collection des services pour chaînage</returns>
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            // ============================================
            // 1. MEDIATR - PATTERN CQRS
            // ============================================
            
            // MediatR scanne TOUS les handlers dans l'assembly Application
            // Il trouve automatiquement :
            // - CreateBandeCommandHandler
            // - UpdateBandeCommandHandler  
            // - DeleteBandeCommandHandler
            // - CloturerBandeCommandHandler
            // - GetAllBandesQueryHandler
            // - GetBandeByIdQueryHandler
            // - GetBandesByStatutQueryHandler
            // - etc.
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // ============================================
            // 2. AUTOMAPPER - MAPPING DTO ↔ ENTITY
            // ============================================
            
            // AutoMapper scanne TOUS les Profiles dans l'assembly Application
            // Il trouve automatiquement :
            // - BandeProfile (mapping Bande ↔ BandeDto)
            // - etc.
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
            // ============================================
            // 3. FLUENTVALIDATION - VALIDATION DES DTOS
            // ============================================
            
            // Active la validation automatique des DTOs
            // Sans ça, les validators ne sont pas appelés automatiquement
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // ============================================
            // 4. PIPELINE BEHAVIORS - COMPORTEMENTS GLOBAUX
            // ============================================
            
            // Ces behaviors s'exécutent AUTOMATIQUEMENT pour TOUTES les Commands/Queries
            
            // 4.1 Validation Pipeline - Valide automatiquement avant le Handler
            // À DÉCOMMENTER QUAND VOUS AUREZ BESOIN DE VALIDATION AUTOMATIQUE
            // services.AddTransient(typeof(IPipelineBehavior<,>), 
            //     typeof(ValidationBehavior<,>));
            
            // 4.2 Logging Pipeline - Logge automatiquement les requêtes
            // services.AddTransient(typeof(IPipelineBehavior<,>), 
            //     typeof(LoggingBehavior<,>));
            
            // 4.3 Performance Pipeline - Mesure le temps d'exécution
            // services.AddTransient(typeof(IPipelineBehavior<,>), 
            //     typeof(PerformanceBehavior<,>));
            
            return services;
        }
    }
}