using Application.Features.Ventes.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Vente (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien VenteProfile (AutoMapper).
    /// </summary>
    public static class VenteMappings
    {
        /// <summary>
        /// Vente (entité) -> VenteResponseDto.
        /// - TypeVente, UniteMesure, ModePaiement : enums convertis en texte + libellé lisible
        /// - BandeCode, ClientNom, CreateurNom : non renseignés ici (null)
        /// </summary>
        public static VenteResponseDto ToResponseDto(this Vente vente)
        {
            return new VenteResponseDto
            {
                Id = vente.Id,
                BandeId = vente.BandeId,
                ClientId = vente.ClientId,
                DateVente = vente.DateVente,
                TypeVente = vente.TypeVente.ToString(),
                TypeVenteLibelle = GetTypeVenteLibelle(vente.TypeVente),
                Quantite = vente.Quantite,
                UniteMesure = vente.UniteMesure.ToString(),
                UniteMesureLibelle = GetUniteMesureVenteLibelle(vente.UniteMesure),
                PrixUnitaire = vente.PrixUnitaire,
                MontantTotal = vente.MontantTotal,
                ModePaiement = vente.ModePaiement.ToString(),
                ModePaiementLibelle = GetModePaiementLibelle(vente.ModePaiement),
                EstPaye = vente.EstPaye,
                DatePaiement = vente.DatePaiement,
                Observations = vente.Observations,
                CreatedAt = vente.CreatedAt,
                CreatedBy = vente.CreatedBy
            };
        }

        /// <summary>Liste de ventes -> liste de DTOs.</summary>
        public static List<VenteResponseDto> ToResponseDtos(this IEnumerable<Vente> ventes)
        {
            return ventes.Select(vente => vente.ToResponseDto()).ToList();
        }

        // ============================================
        // Libellés lisibles (affichés à l'utilisateur)
        // ============================================

        private static string GetTypeVenteLibelle(TypeVente type) => type switch
        {
            TypeVente.PouletsVivants => "Poulets vivants",
            TypeVente.PouletsNettoyes => "Poulets nettoyés",
            TypeVente.PouletsFumes => "Poulets fumés",
            TypeVente.Fientes => "Fientes",
            TypeVente.Autres => "Autres",
            _ => "Inconnu"
        };

        private static string GetUniteMesureVenteLibelle(UniteMesureVente unite) => unite switch
        {
            UniteMesureVente.Unite => "Unité",
            UniteMesureVente.Kg => "Kilogramme",
            UniteMesureVente.Sac => "Sac",
            UniteMesureVente.Camion => "Camion",
            UniteMesureVente.Rouleau => "Rouleau",
            UniteMesureVente.Paquet => "Paquet",
            _ => unite.ToString()
        };

        private static string GetModePaiementLibelle(ModePaiement mode) => mode switch
        {
            ModePaiement.Especes => "Espèces",
            ModePaiement.PaiementMobile => "Paiement Mobile",
            ModePaiement.VirementBancaire => "Virement bancaire",
            ModePaiement.Cheque => "Chèque",
            ModePaiement.Credit => "Crédit",
            _ => mode.ToString()
        };
    }
}
