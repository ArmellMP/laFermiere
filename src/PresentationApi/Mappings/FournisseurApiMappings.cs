using Application.Features.Fournisseurs.Commands.CreateFournisseur;
using Application.Features.Fournisseurs.Commands.UpdateFournisseur;
using Application.Features.Fournisseurs.Dtos;
using PresentationApi.Models.Requests.Fournisseurs;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les fournisseurs.
    /// Remplace l'ancien Profiles/FournisseurProfile (AutoMapper).
    /// </summary>
    public static class FournisseurApiMappings
    {
        /// <summary>CreateFournisseurRequest -> CreateFournisseurCommand.</summary>
        public static CreateFournisseurCommand ToCommand(this CreateFournisseurRequest request)
        {
            return new CreateFournisseurCommand
            {
                CreateFournisseurDto = new CreateFournisseurDto
                {
                    Nom = request.Nom,
                    TypeFournisseur = request.TypeFournisseur,
                    Telephone = request.Telephone,
                    Email = request.Email,
                    Adresse = request.Adresse,
                    ContactPersonne = request.ContactPersonne,
                    Notes = request.Notes
                }
            };
        }

        /// <summary>
        /// UpdateFournisseurRequest -> UpdateFournisseurCommand.
        /// Les champs laissés à null signifient "ne pas modifier".
        /// </summary>
        public static UpdateFournisseurCommand ToCommand(this UpdateFournisseurRequest request)
        {
            return new UpdateFournisseurCommand
            {
                UpdateFournisseurDto = new UpdateFournisseurDto
                {
                    Id = request.Id,
                    Nom = request.Nom,
                    TypeFournisseur = request.TypeFournisseur,
                    Telephone = request.Telephone,
                    Email = request.Email,
                    Adresse = request.Adresse,
                    ContactPersonne = request.ContactPersonne,
                    Notes = request.Notes,
                    EstActif = request.EstActif
                }
            };
        }

        /// <summary>FournisseurResponseDto -> FournisseurResponse (JSON renvoyé au client HTTP).</summary>
        public static FournisseurResponse ToResponse(this FournisseurResponseDto dto)
        {
            return new FournisseurResponse
            {
                Id = dto.Id,
                Nom = dto.Nom,
                TypeFournisseur = dto.TypeFournisseur,
                TypeFournisseurLibelle = dto.TypeFournisseurLibelle,
                Telephone = dto.Telephone,
                Email = dto.Email,
                Adresse = dto.Adresse,
                ContactPersonne = dto.ContactPersonne,
                Notes = dto.Notes,
                EstActif = dto.EstActif,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                // int / decimal -> int? / decimal? : la valeur est toujours présente (0 par défaut)
                NombreAchats = dto.NombreAchats,
                MontantTotalAchats = dto.MontantTotalAchats
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<FournisseurResponse> ToResponses(this IEnumerable<FournisseurResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
