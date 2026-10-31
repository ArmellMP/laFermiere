using MediatR;
using Application.Features.Achats.Dtos;

namespace Application.Features.Achats.Queries.GetAchatsByFournisseur
{
    public record GetAchatsByFournisseurQuery : IRequest<List<AchatResponseDto>>
    {
        public int FournisseurId { get; init; }
    }
}