using Application.Features.Clients.Commands.CreateClient;
using Application.Features.Clients.Commands.UpdateClient;
using Application.Features.Clients.Dtos;
using PresentationApi.Models.Requests.Clients;
using PresentationApi.Models.Responses;

namespace PresentationApi.Mappings
{
    /// <summary>
    /// Conversions HTTP (Requests / Responses) <-> Application (Commands / DTOs) pour les clients.
    /// Remplace l'ancien Profiles/ClientProfile (AutoMapper).
    /// </summary>
    public static class ClientApiMappings
    {
        /// <summary>CreateClientRequest -> CreateClientCommand (les données vont dans CreateClientDto).</summary>
        public static CreateClientCommand ToCommand(this CreateClientRequest request)
        {
            return new CreateClientCommand
            {
                CreateClientDto = new CreateClientDto
                {
                    Nom = request.Nom,
                    TypeClient = request.TypeClient,
                    Telephone = request.Telephone,
                    Email = request.Email,
                    Adresse = request.Adresse,
                    Notes = request.Notes
                }
            };
        }

        /// <summary>
        /// UpdateClientRequest -> UpdateClientCommand.
        /// Les champs laissés à null dans la requête signifient "ne pas modifier".
        /// </summary>
        public static UpdateClientCommand ToCommand(this UpdateClientRequest request)
        {
            return new UpdateClientCommand
            {
                UpdateClientDto = new UpdateClientDto
                {
                    Id = request.Id,
                    Nom = request.Nom,
                    TypeClient = request.TypeClient,
                    Telephone = request.Telephone,
                    Email = request.Email,
                    Adresse = request.Adresse,
                    Notes = request.Notes,
                    EstActif = request.EstActif
                }
            };
        }

        /// <summary>ClientResponseDto -> ClientResponse (JSON renvoyé au client HTTP).</summary>
        public static ClientResponse ToResponse(this ClientResponseDto dto)
        {
            return new ClientResponse
            {
                Id = dto.Id,
                Nom = dto.Nom,
                TypeClient = dto.TypeClient,
                TypeClientLibelle = dto.TypeClientLibelle,
                Telephone = dto.Telephone,
                Email = dto.Email,
                Adresse = dto.Adresse,
                Notes = dto.Notes,
                EstActif = dto.EstActif,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                // int -> int? : la valeur est toujours présente (0 si aucune vente)
                NombreVentes = dto.NombreVentes,
                MontantTotalAchats = dto.MontantTotalAchats
            };
        }

        /// <summary>Liste de DTOs -> liste de réponses.</summary>
        public static List<ClientResponse> ToResponses(this IEnumerable<ClientResponseDto> dtos)
        {
            return dtos.Select(dto => dto.ToResponse()).ToList();
        }
    }
}
