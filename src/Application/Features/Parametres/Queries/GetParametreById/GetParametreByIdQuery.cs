using MediatR;
using Application.Features.Parametres.Dtos;

namespace Application.Features.Parametres.Queries.GetParametreById
{
    public record GetParametreByIdQuery : IRequest<ParametreResponseDto>
    {
        public int Id { get; init; }
    }
}