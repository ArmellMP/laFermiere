using MediatR;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Commands.UpdateParametre
{
    public record UpdateParametreCommand : IRequest<ParametreResponseDto>
    {
        public UpdateParametreDto UpdateParametreDto { get; init; } = null!;
    }
}