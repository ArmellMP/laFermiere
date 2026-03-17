using Application.Common.Exceptions;
using Application.Features.Bandes.Dtos;
using Application.Features.Bandes.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;

namespace Application.Features.Bandes.Commands.CloturerBande
{
    /// <summary>
    /// Handler pour clôturer une bande.
    /// Opération métier importante avec beaucoup de validations.
    /// </summary>
    public class CloturerBandeCommandHandler 
        : IRequestHandler<CloturerBandeCommand, BandeResponseDto>
    {
        private readonly IBandeRepository _bandeRepository;
        private readonly IMapper _mapper;

        public CloturerBandeCommandHandler(
            IBandeRepository bandeRepository,
            IMapper mapper)
        {
            _bandeRepository = bandeRepository;
            _mapper = mapper;
        }

        public async Task<BandeResponseDto> Handle(
            CloturerBandeCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Récupérer la bande
            var bande = await GetBandeAsync(request.Id, cancellationToken);
            
            // 2. Valider qu'on peut clôturer cette bande
            ValidateCanCloture(bande);
            
            // 3. Appliquer la clôture
            ApplyCloture(bande, request);
            
            // 4. Mettre à jour le timestamp
            bande.UpdatedAt = DateTime.Now;
            
            // 5. Sauvegarder
            await _bandeRepository.UpdateAsync(bande, cancellationToken);
            
            // 6. Convertir en DTO pour la réponse
            var bandeDto = _mapper.Map<BandeResponseDto>(bande);
            bandeDto.IncludeDetails = true; // Toujours inclure les détails après clôture
            
            return bandeDto;
        }
        
        /// <summary>
        /// Récupère une bande ou lance NotFoundException.
        /// </summary>
        private async Task<Domain.Entities.Bande> GetBandeAsync(
            int bandeId, 
            CancellationToken cancellationToken)
        {
            var bande = await _bandeRepository.GetByIdAsync(bandeId, cancellationToken);
            
            if (bande == null)
            {
                throw new NotFoundException(
                    $"Bande avec l'ID {bandeId} non trouvée.");
            }
            
            return bande;
        }
        
        /// <summary>
        /// Valide qu'on peut clôturer cette bande.
        /// </summary>
        private void ValidateCanCloture(Domain.Entities.Bande bande)
        {
            // Règle 1 : Ne peut clôturer que les bandes "EnCours"
            if (bande.Statut != StatutBande.EnCours)
            {
                var statutLibelle = GetStatutLibelle(bande.Statut);
                throw new BusinessRuleException(
                    $"Impossible de clôturer une bande avec le statut '{statutLibelle}'. " +
                    "Seules les bandes 'En cours' peuvent être clôturées.");
            }
            
            // Règle 2 : Vérifier le nombre de poussins
            if (bande.NbPoussinsActuel <= 0)
            {
                throw new BusinessRuleException(
                    "Impossible de clôturer une bande sans poussins.");
            }
            
            // Règle 3 : Vérifier la date d'arrivée (au moins 1 jour)
            var joursEcoules = (DateTime.Now - bande.DateArrivee).Days;
            if (joursEcoules < 1)
            {
                throw new BusinessRuleException(
                    "Impossible de clôturer une bande le jour de son arrivée.");
            }
            
            // Règle 4 : Vérifier si le cycle est trop court (< 30 jours)
            // (cycle normal = 45 jours, mais minimum 30)
            if (joursEcoules < 30)
            {
                throw new BusinessRuleException(
                    $"Cycle trop court ({joursEcoules} jours). " +
                    "Le cycle minimum est de 30 jours.");
            }
            
            // Règle 5 : Vérifier le taux de mortalité (maximum 20%)
            var tauxMortalite = CalculateTauxMortalite(bande);
            if (tauxMortalite > 20)
            {
                throw new BusinessRuleException(
                    $"Taux de mortalité trop élevé ({tauxMortalite:F1}%). " +
                    "Le maximum autorisé est de 20%.");
            }
            
            // Règle 6 : Vérifier s'il y a des suivis journaliers manquants
            // (Pour l'instant, on suppose que tout est OK)
            // TODO: Implémenter cette vérification quand on aura les suivis
            
            // Règle 7 : Vérifier si la bande a des stocks non consommés
            // (Pour l'instant, on suppose que tout est OK)
            // TODO: Implémenter cette vérification quand on aura les stocks
        }
        
        /// <summary>
        /// Applique la clôture à la bande.
        /// </summary>
        private void ApplyCloture(
            Domain.Entities.Bande bande, 
            CloturerBandeCommand request)
        {
            // 1. Changer le statut
            bande.Statut = StatutBande.Cloturee;
            
            // 2. Définir la date de clôture
            bande.DateCloture = request.DateCloture ?? DateTime.Now;
            
            // 3. Vérifier que la date de clôture n'est pas antérieure à la date d'arrivée
            if (bande.DateCloture < bande.DateArrivee)
            {
                throw new BusinessRuleException(
                    "La date de clôture ne peut être antérieure à la date d'arrivée.");
            }
            
            // 4. Ajouter les observations de clôture
            if (!string.IsNullOrEmpty(request.ObservationsCloture))
            {
                var observationsExistantes = bande.Observations ?? string.Empty;
                var separateur = string.IsNullOrEmpty(observationsExistantes) ? "" : "\n\n";
                var observationsCloture = $"{separateur}=== CLÔTURE ===\n" +
                                         $"Date: {bande.DateCloture:dd/MM/yyyy}\n" +
                                         $"Observations: {request.ObservationsCloture}";
                bande.Observations = observationsExistantes + observationsCloture;
            }
            
            // 5. Calculer les statistiques finales (optionnel, pour référence)
            // On pourrait calculer et stocker :
            // - Poids moyen final
            // - Consommation totale d'aliment
            // - Coût total
            // - etc.
        }
        
        /// <summary>
        /// Calcule le taux de mortalité.
        /// </summary>
        private decimal CalculateTauxMortalite(Domain.Entities.Bande bande)
        {
            if (bande.NbPoussinsInit <= 0)
                return 0;
                
            var mortalites = bande.NbPoussinsInit - bande.NbPoussinsActuel;
            return (mortalites / (decimal)bande.NbPoussinsInit) * 100;
        }
        
        /// <summary>
        /// Convertit un statut en libellé lisible.
        /// </summary>
        private string GetStatutLibelle(StatutBande statut)
        {
            return statut switch
            {
                StatutBande.EnPreparation => "En préparation",
                StatutBande.EnCours => "En cours",
                StatutBande.Cloturee => "Clôturée",
                StatutBande.Archivee => "Archivée",
                _ => "Inconnu"
            };
        }
    }
}