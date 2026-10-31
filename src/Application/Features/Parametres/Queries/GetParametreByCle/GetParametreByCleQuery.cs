using MediatR;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Queries.GetParametreByCle
{
    public record GetParametreByCleQuery : IRequest<ParametreResponseDto>
    {
        public string Cle { get; init; } = string.Empty;
    }
}