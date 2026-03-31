using MediatR;
using Application.Features.Fournisseurs.Dtos;

namespace Application.Features.Fournisseurs.Commands.CreateFournisseur
{
    public record CreateFournisseurCommand : IRequest<FournisseurResponseDto>
    {
        public CreateFournisseurDto CreateFournisseurDto { get; init; } = null!;
    }
}