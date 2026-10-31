using Application.Features.Ventes.Commands.CreateVente;
using Application.Features.Ventes.Commands.UpdateVente;
using Application.Features.Ventes.Dtos;
using PresentationApi.Models.Requests.Ventes;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les ventes.
    /// Remplace l'ancien Profiles/VenteProfile (AutoMapper).
    /// </summary>
    public static class VenteApiMappings
    {
        /// <summary>CreateVenteRequest -> CreateVenteCommand (les données vont dans CreateVenteDto).</summary>
        public static CreateVenteCommand ToCommand(this CreateVenteRequest request)
        {
            return new CreateVenteCommand
            {
                CreateVenteDto = new CreateVenteDto
                {
                    BandeId = request.BandeId,
                    ClientId = request.ClientId,
                    DateVente = request.DateVente,
                    TypeVente = request.TypeVente,
                    Quantite = request.Quantite,
                    UniteMesure = request.UniteMesure,
                    PrixUnitaire = request.PrixUnitaire,
                    ModePaiement = request.ModePaiement,
                    EstPaye = request.EstPaye,
                    DatePaiement = request.DatePaiement,
                    Observations = request.Observations
                }
            };
        }

        /// <summary>
        /// UpdateVenteRequest -> UpdateVenteCommand.
        /// Les champs laissés à null signifient "ne pas modifier".
        /// </summary>
        public static UpdateVenteCommand ToCommand(this UpdateVenteRequest request)
        {
            return new UpdateVenteCommand
            {
                UpdateVenteDto = new UpdateVenteDto
                {
                    Id = request.Id,
                    DateVente = request.DateVente,
                    TypeVente = request.TypeVente,
                    Quantite = request.Quantite,
                    UniteMesure = request.UniteMesure,
                    PrixUnitaire = request.PrixUnitaire,
                    ModePaiement = request.ModePaiement,
                    EstPaye = request.EstPaye,
                    DatePaiement = request.DatePaiement,
                    Observations = request.Observations
                }
            };
        }

        /// <summary>VenteResponseDto -> VenteResponse (JSON renvoyé au client HTTP).</summary>
        public static VenteResponse ToResponse(this VenteResponseDto dto)
        {
            return new VenteResponse
            {
                Id = dto.Id,
                BandeId = dto.BandeId,
                BandeCode = dto.BandeCode,
                ClientId = dto.ClientId,
                ClientNom = dto.ClientNom,
                DateVente = dto.DateVente,
                TypeVente = dto.TypeVente,
                TypeVenteLibelle = dto.TypeVenteLibelle,
                Quantite = dto.Quantite,
                UniteMesure = dto.UniteMesure,
                UniteMesureLibelle = dto.UniteMesureLibelle,
                PrixUnitaire = dto.PrixUnitaire,
                MontantTotal = dto.MontantTotal,
                ModePaiement = dto.ModePaiement,
                ModePaiementLibelle = dto.ModePaiementLibelle,
                EstPaye = dto.EstPaye,
                DatePaiement = dto.DatePaiement,
                Observations = dto.Observations,
                CreatedAt = dto.CreatedAt,
                CreatedBy = dto.CreatedBy,
                CreateurNom = dto.CreateurNom
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<VenteResponse> ToResponses(this IEnumerable<VenteResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
