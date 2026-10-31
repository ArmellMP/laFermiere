using Application.Features.Parametres.Commands.CreateParametre;
using Application.Features.Parametres.Commands.UpdateParametre;
using Application.Features.Parametres.Dtos;
using PresentationApi.Models.Requests.Parametres;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les paramètres.
    /// Remplace l'ancien Profiles/ParametreProfile (AutoMapper).
    /// </summary>
    public static class ParametreApiMappings
    {
        /// <summary>CreateParametreRequest -> CreateParametreCommand.</summary>
        public static CreateParametreCommand ToCommand(this CreateParametreRequest request)
        {
            return new CreateParametreCommand
            {
                CreateParametreDto = new CreateParametreDto
                {
                    Cle = request.Cle,
                    Valeur = request.Valeur,
                    Description = request.Description,
                    Categorie = request.Categorie,
                    EstModifiable = request.EstModifiable
                }
            };
        }

        /// <summary>
        /// UpdateParametreRequest -> UpdateParametreCommand.
        /// La clé (Cle) ne se modifie pas : elle n'est pas dans la requête de mise à jour.
        /// </summary>
        public static UpdateParametreCommand ToCommand(this UpdateParametreRequest request)
        {
            return new UpdateParametreCommand
            {
                UpdateParametreDto = new UpdateParametreDto
                {
                    Id = request.Id,
                    Valeur = request.Valeur,
                    Description = request.Description,
                    Categorie = request.Categorie,
                    EstModifiable = request.EstModifiable
                }
            };
        }

        /// <summary>ParametreResponseDto -> ParametreResponse (JSON renvoyé au client HTTP).</summary>
        public static ParametreResponse ToResponse(this ParametreResponseDto dto)
        {
            return new ParametreResponse
            {
                Id = dto.Id,
                Cle = dto.Cle,
                Valeur = dto.Valeur,
                Description = dto.Description,
                Categorie = dto.Categorie,
                CategorieLibelle = dto.CategorieLibelle,
                EstModifiable = dto.EstModifiable,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<ParametreResponse> ToResponses(this IEnumerable<ParametreResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
