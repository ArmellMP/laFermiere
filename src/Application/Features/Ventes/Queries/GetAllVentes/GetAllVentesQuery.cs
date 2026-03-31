using MediatR;
using Application.Features.Ventes.Dtos;

namespace Application.Features.Ventes.Queries.GetAllVentes
{
    public record GetAllVentesQuery : IRequest<List<VenteResponseDto>>
    {
        /// <summary>
        /// Filtrer par bande (optionnel)
        /// </summary>
        public int? BandeId { get; init; }
        
        /// <summary>
        /// Filtrer par client (optionnel)
        /// </summary>
        public int? ClientId { get; init; }
        
        /// <summary>
        /// Filtrer par type de vente (optionnel)
        /// </summary>
        public string? TypeVente { get; init; }
        
        /// <summary>
        /// Filtrer par statut de paiement (optionnel)
        /// </summary>
        public bool? EstPaye { get; init; }
        
        /// <summary>
        /// Date de début (optionnel)
        /// </summary>
        public DateTime? DateDebut { get; init; }
        
        /// <summary>
        /// Date de fin (optionnel)
        /// </summary>
        public DateTime? DateFin { get; init; }
    }
}