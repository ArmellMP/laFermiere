using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepClientEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string TypeClient { get; set; } = string.Empty;  // string (SQL ENUM)
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? Notes { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS (optionnel) ====
        public int? NombreVentes { get; set; }
        public decimal? MontantTotalAchats { get; set; }

        /// <summary>
        /// Convertit RepClientEntity en entité Domain Client. Conversion string → enum.
        /// </summary>
        public Client ToDomain()
        {
            TypeClient typeClient = Domain.Enums.TypeClient.Particulier;
            
            if (!string.IsNullOrEmpty(TypeClient))
            {
                if (Enum.TryParse<Domain.Enums.TypeClient>(TypeClient, true, out var parsed))
                {
                    typeClient = parsed;
                }
            }

            return new Client
            {
                Id = this.Id,
                Nom = this.Nom,
                TypeClient = typeClient,
                Telephone = this.Telephone,
                Email = this.Email,
                Adresse = this.Adresse,
                Notes = this.Notes,
                EstActif = this.EstActif,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }

        /// <summary>
        /// Convertit une entité Domain Client en RepClientEntity. Conversion enum → string.
        /// </summary>
        public static RepClientEntity FromDomain(Client client)
        {
            return new RepClientEntity
            {
                Id = client.Id,
                Nom = client.Nom,
                TypeClient = client.TypeClient.ToString(),
                Telephone = client.Telephone,
                Email = client.Email,
                Adresse = client.Adresse,
                Notes = client.Notes,
                EstActif = client.EstActif,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
        }

        /// <summary>
        /// Conversion implicite vers l'entité Domain
        /// </summary>
        public static implicit operator Client?(RepClientEntity? rep)
        {
            return rep?.ToDomain();
        }

        /// <summary>
        /// Conversion implicite depuis l'entité Domain
        /// </summary>
        public static implicit operator RepClientEntity?(Client? client)
        {
            return client == null ? null : FromDomain(client);
        }
    }
}