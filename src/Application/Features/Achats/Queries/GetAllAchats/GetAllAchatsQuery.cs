using MediatR;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAllAchats
{
    public record GetAllAchatsQuery : IRequest<List<AchatResponseDto>>
    {
        /// <summary>
        /// Filtrer par bande (optionnel)
        /// </summary>
        public int? BandeId { get; init; }
        
        /// <summary>
        /// Filtrer par fournisseur (optionnel)
        /// </summary>
        public int? FournisseurId { get; init; }
        
        /// <summary>
        /// Filtrer par type d'achat (optionnel)
        /// </summary>
        public string? TypeAchat { get; init; }
        
        /// <summary>
        /// Uniquement les achats non reçus
        /// </summary>
        public bool? OnlyNonRecus { get; init; }
        
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