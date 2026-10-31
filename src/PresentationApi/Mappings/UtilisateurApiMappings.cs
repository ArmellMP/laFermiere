using Application.Features.Utilisateurs.Commands.CreateUtilisateur;
using Application.Features.Utilisateurs.Commands.UpdateUtilisateur;
using Application.Features.Utilisateurs.Dtos;
using PresentationApi.Models.Requests.Utilisateurs;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les utilisateurs.
    /// Remplace l'ancien Profiles/UtilisateurProfile (AutoMapper).
    /// </summary>
    public static class UtilisateurApiMappings
    {
        /// <summary>
        /// CreateUtilisateurRequest -> CreateUtilisateurCommand.
        /// Le mot de passe passe en clair jusqu'au handler, qui le hache (BCrypt) avant tout stockage.
        /// </summary>
        public static CreateUtilisateurCommand ToCommand(this CreateUtilisateurRequest request)
        {
            return new CreateUtilisateurCommand
            {
                CreateUtilisateurDto = new CreateUtilisateurDto
                {
                    Nom = request.Nom,
                    Email = request.Email,
                    Password = request.Password,
                    Role = request.Role
                }
            };
        }

        /// <summary>
        /// UpdateUtilisateurRequest -> UpdateUtilisateurCommand.
        /// Les champs laissés à null signifient "ne pas modifier" (dont le mot de passe).
        /// </summary>
        public static UpdateUtilisateurCommand ToCommand(this UpdateUtilisateurRequest request)
        {
            return new UpdateUtilisateurCommand
            {
                UpdateUtilisateurDto = new UpdateUtilisateurDto
                {
                    Id = request.Id,
                    Nom = request.Nom,
                    Email = request.Email,
                    Password = request.Password,
                    Role = request.Role,
                    EstActif = request.EstActif
                }
            };
        }

        /// <summary>UtilisateurResponseDto -> UtilisateurResponse (JSON renvoyé, sans aucun mot de passe).</summary>
        public static UtilisateurResponse ToResponse(this UtilisateurResponseDto dto)
        {
            return new UtilisateurResponse
            {
                Id = dto.Id,
                Nom = dto.Nom,
                Email = dto.Email,
                Role = dto.Role,
                RoleLibelle = dto.RoleLibelle,
                EstActif = dto.EstActif,
                DerniereConnexion = dto.DerniereConnexion,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                EstAdmin = dto.EstAdmin,
                EstFermier = dto.EstFermier,
                EstVeterinaire = dto.EstVeterinaire,
                EstComptable = dto.EstComptable
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<UtilisateurResponse> ToResponses(this IEnumerable<UtilisateurResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
