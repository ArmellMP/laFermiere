using Microsoft.Extensions.DependencyInjection;
using System.Reflection;// Pour Assembly.GetExecutingAssembly()
using FluentValidation; // Pour la validation automatique

namespace Application
{
    /// <summary>
    /// Configuration des services pour la couche Application.
    /// Responsable de l'enregistrement de tous les services liés à :
    /// - MediatR (CQRS : Commands/Queries/Handlers)
    /// - FluentValidation (Validation des DTOs)
    ///
    /// Le mapping Entité -> DTO n'a pas besoin d'être enregistré ici : ce sont des
    /// méthodes d'extension statiques (Common/Mappings/XxxMappings.cs), appelées directement.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Ajoute tous les services de la couche Application.
        /// Cette méthode sera appelée depuis PresentationApi.
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
            // 2. FLUENTVALIDATION - VALIDATION DES DTOS
            // ============================================
            
            // Active la validation automatique des DTOs
            // Sans ça, les validators ne sont pas appelés automatiquement
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // ============================================
            // 3. PIPELINE BEHAVIORS - COMPORTEMENTS GLOBAUX
            // ============================================
            
            // Ces behaviors s'exécutent AUTOMATIQUEMENT pour TOUTES les Commands/Queries
            
            // 3.1 Validation Pipeline - Valide automatiquement avant le Handler
            // À DÉCOMMENTER QUAND VOUS AUREZ BESOIN DE VALIDATION AUTOMATIQUE
            // services.AddTransient(typeof(IPipelineBehavior<,>), 
            //     typeof(ValidationBehavior<,>));
            
            // 3.2 Logging Pipeline - Logge automatiquement les requêtes
            // services.AddTransient(typeof(IPipelineBehavior<,>), 
            //     typeof(LoggingBehavior<,>));
            
            // 3.3 Performance Pipeline - Mesure le temps d'exécution
            // services.AddTransient(typeof(IPipelineBehavior<,>), 
            //     typeof(PerformanceBehavior<,>));
            
            return services;
        }
    }
}