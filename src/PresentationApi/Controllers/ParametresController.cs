using PresentationApi.Security;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PresentationApi.Models.Requests.Parametres;
using PresentationApi.Models.Responses;
using Application.Features.Parametres.Queries.GetAllParametres;
using Application.Features.Parametres.Queries.GetParametreById;
using Application.Features.Parametres.Queries.GetParametreByCle;
using Application.Features.Parametres.Commands.CreateParametre;
using Application.Features.Parametres.Commands.UpdateParametre;
using Application.Features.Parametres.Commands.DeleteParametre;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ParametresController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ParametresController> _logger;

        public ParametresController(
            IMediator mediator,
            ILogger<ParametresController> logger)
        {
            _mediator = mediator;
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
            var response = result.ToResponses();
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
            var response = result.ToResponse();
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
            var response = result.ToResponse();
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

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
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
                // Exception plutôt que BadRequest("texte") : ExceptionMiddleware renvoie le format commun ErrorResponse (400)
                throw new BusinessRuleException("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour du paramètre {ParametreId}", id);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
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