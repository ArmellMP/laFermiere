using Application.Features.Achats.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Achat (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien AchatProfile (AutoMapper).
    /// </summary>
    public static class AchatMappings
    {
        /// <summary>
        /// Achat (entité) -> AchatResponseDto.
        /// - TypeAchat, UniteMesure : enums convertis en texte + libellé lisible
        /// - EstRecu, JoursEntreAchatEtReception : valeurs calculées par l'entité
        /// - FournisseurNom, BandeCode, CreateurNom : non renseignés ici (null)
        /// </summary>
        public static AchatResponseDto ToResponseDto(this Achat achat)
        {
            return new AchatResponseDto
            {
                Id = achat.Id,
                FournisseurId = achat.FournisseurId,
                BandeId = achat.BandeId,
                DateAchat = achat.DateAchat,
                TypeAchat = achat.TypeAchat.ToString(),
                TypeAchatLibelle = GetTypeAchatLibelle(achat.TypeAchat),
                Description = achat.Description,
                Quantite = achat.Quantite,
                UniteMesure = achat.UniteMesure.ToString(),
                UniteMesureLibelle = GetUniteMesureAchatLibelle(achat.UniteMesure),
                PrixUnitaire = achat.PrixUnitaire,
                MontantTotal = achat.MontantTotal,
                FactureNumero = achat.FactureNumero,
                DateReception = achat.DateReception,
                EstRecu = achat.EstRecu,
                JoursEntreAchatEtReception = achat.JoursEntreAchatEtReception,
                Observations = achat.Observations,
                CreatedAt = achat.CreatedAt,
                CreatedBy = achat.CreatedBy
            };
        }

        /// <summary>Liste d'achats -> liste de DTOs.</summary>
        public static List<AchatResponseDto> ToResponseDtos(this IEnumerable<Achat> achats)
        {
            return achats.Select(achat => achat.ToResponseDto()).ToList();
        }

        // ============================================
        // Libellés lisibles (affichés à l'utilisateur)
        // ============================================

        private static string GetTypeAchatLibelle(TypeAchat type) => type switch
        {
            TypeAchat.Poussins => "Poussins",
            TypeAchat.Aliment => "Aliment",
            TypeAchat.Medicament => "Médicament",
            TypeAchat.Materiel => "Matériel",
            TypeAchat.Services => "Services",
            TypeAchat.Transport => "Transport",
            TypeAchat.Autre => "Autre",
            _ => "Inconnu"
        };

        private static string GetUniteMesureAchatLibelle(UniteMesureAchat unite) => unite switch
        {
            UniteMesureAchat.Unite => "Unité",
            UniteMesureAchat.Kg => "Kilogramme",
            UniteMesureAchat.L => "Litre",
            UniteMesureAchat.Sac => "Sac",
            UniteMesureAchat.Dose => "Dose",
            UniteMesureAchat.Heure => "Heure",
            UniteMesureAchat.Km => "Kilomètre",
            UniteMesureAchat.Paquet => "Paquet",
            _ => unite.ToString()
        };
    }
}
