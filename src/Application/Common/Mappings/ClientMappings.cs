using Application.Features.Clients.Dtos;
using Domain.Entities;

namespace Application.Common.Mappings
{
    /// <summary>
    /// Conversions Client (entité) -> DTOs de la couche Application.
    /// Remplace l'ancien ClientProfile (AutoMapper).
    /// </summary>
    public static class ClientMappings
    {
        /// <summary>
        /// Client (entité) -> ClientResponseDto.
        /// - TypeClient : enum converti en texte ("Restaurant")
        /// - TypeClientLibelle : texte lisible calculé par l'entité ("Restaurant / Hôtel"...)
        /// - NombreVentes / MontantTotalAchats : restent à 0 ici ; un handler peut les remplir
        /// </summary>
        public static ClientResponseDto ToResponseDto(this Client client)
        {
            return new ClientResponseDto
            {
                Id = client.Id,
                Nom = client.Nom,
                TypeClient = client.TypeClient.ToString(),
                TypeClientLibelle = client.TypeClientLibelle,
                Telephone = client.Telephone,
                Email = client.Email,
                Adresse = client.Adresse,
                Notes = client.Notes,
                EstActif = client.EstActif,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
        }

        /// <summary>Liste de clients -> liste de DTOs.</summary>
        public static List<ClientResponseDto> ToResponseDtos(this IEnumerable<Client> clients)
        {
            return clients.Select(client => client.ToResponseDto()).ToList();
        }
    }
}
