using MediatR;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Commands.UpdateFournisseur
{
    public record UpdateFournisseurCommand : IRequest<FournisseurResponseDto>
    {
        public UpdateFournisseurDto UpdateFournisseurDto { get; init; } = null!;
    }
}