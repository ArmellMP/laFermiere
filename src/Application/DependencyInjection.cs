using Microsoft.Extensions.DependencyInjection;
using System.Reflection;// Pour Assembly.GetExecutingAssembly()
using FluentValidation; // Pour AddValidatorsFromAssembly
using Application.Common.Behaviors; // Pour ValidationBehavior

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
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // PIPELINE BEHAVIOR - VALIDATION AUTOMATIQUE
                // Chaque Command/Query passe d'abord par ValidationBehavior,
                // qui exécute ses validateurs FluentValidation avant le Handler.
                // "Open" = générique : un seul enregistrement couvre tous les types de requêtes.
                // (D'autres behaviors pourront s'ajouter ici : journalisation, mesure du temps...)
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // ============================================
            // 2. FLUENTVALIDATION - ENREGISTREMENT DES VALIDATEURS
            // ============================================

            // Enregistre toutes les classes AbstractValidator<T> de l'assembly Application
            // dans l'injection de dépendances. Attention : cela les rend DISPONIBLES,
            // mais ne les EXÉCUTE pas. C'est ValidationBehavior (ci-dessus) qui les appelle.
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}