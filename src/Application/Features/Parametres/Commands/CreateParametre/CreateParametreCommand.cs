using MediatR;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Commands.CreateParametre
{
    public record CreateParametreCommand : IRequest<ParametreResponseDto>
    {
        public CreateParametreDto CreateParametreDto { get; init; } = null!;
    }
}