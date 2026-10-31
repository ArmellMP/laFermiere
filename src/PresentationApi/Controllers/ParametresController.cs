using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PresentationApi.Models.Requests.Parametres;
using PresentationApi.Models.Responses;
using Application.Features.Parametres.Queries.GetAllParametres;
using Application.Features.Parametres.Queries.GetParametreById;
using Application.Features.Parametres.Queries.GetParametreByCle;
using Application.Features.Parametres.Commands.CreateParametre;
using Application.Features.Parametres.Commands.UpdateParametre;
using Application.Features.Parametres.Commands.DeleteParametre;

namespace PresentationApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ParametresController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ParametresController> _logger;

        public ParametresController(
            IMediator mediator,
            IMapper mapper,
            ILogger<ParametresController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        [HttpGet]
        [ProducesResponseType(typeof(List<ParametreResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? categorie = null,
            [FromQuery] bool? onlyModifiables = null,
            [FromQuery] string? searchTerm = null)
        {
            _logger.LogInformation("Récupération de la liste des paramètres");

            var query = new GetAllParametresQuery
            {
                Categorie = categorie,
                OnlyModifiables = onlyModifiables,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            var response = _mapper.Map<List<ParametreResponse>>(result);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ParametreResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération du paramètre {ParametreId}", id);

            var query = new GetParametreByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            var response = _mapper.Map<ParametreResponse>(result);
            return Ok(response);
        }

        [HttpGet("cle/{cle}")]
        [ProducesResponseType(typeof(ParametreResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByCle(string cle)
        {
            _logger.LogInformation("Récupération du paramètre par clé : {Cle}", cle);

            var query = new GetParametreByCleQuery { Cle = cle };
            var result = await _mediator.Send(query);
            var response = _mapper.Map<ParametreResponse>(result);
            return Ok(response);
        }

        // ============================================
        // COMMANDS
        // ============================================

        [HttpPost]
        [ProducesResponseType(typeof(ParametreResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateParametreRequest request)
        {
            _logger.LogInformation("Création d'un nouveau paramètre: {Cle}", request.Cle);

            var command = _mapper.Map<CreateParametreCommand>(request);
            var result = await _mediator.Send(command);

            var response = _mapper.Map<ParametreResponse>(result);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ParametreResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateParametreRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour du paramètre {ParametreId}", id);

            var command = _mapper.Map<UpdateParametreCommand>(request);
            var result = await _mediator.Send(command);

            var response = _mapper.Map<ParametreResponse>(result);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression du paramètre {ParametreId}", id);

            var command = new DeleteParametreCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}