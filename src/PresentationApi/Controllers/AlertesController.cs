using PresentationApi.Security;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PresentationApi.Models.Requests.Alertes;
using PresentationApi.Models.Responses;
using Application.Features.Alertes.Queries.GetAllAlertes;
using Application.Features.Alertes.Queries.GetAlerteById;
using Application.Features.Alertes.Queries.GetAlertesNonLues;
using Application.Features.Alertes.Queries.GetAlertesStats;
using Application.Features.Alertes.Commands.CreateAlerte;
using Application.Features.Alertes.Commands.UpdateAlerte;
using Application.Features.Alertes.Commands.DeleteAlerte;
using Application.Features.Alertes.Commands.MarquerAlertes;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AlertesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AlertesController> _logger;

        public AlertesController(
            IMediator mediator,
            ILogger<AlertesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        [HttpGet]
        [ProducesResponseType(typeof(List<AlerteResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? typeAlerte = null,
            [FromQuery] string? niveau = null,
            [FromQuery] bool? onlyNonLues = null,
            [FromQuery] int? entiteId = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération de la liste des alertes");

            var query = new GetAllAlertesQuery
            {
                TypeAlerte = typeAlerte,
                Niveau = niveau,
                OnlyNonLues = onlyNonLues,
                EntiteId = entiteId,
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("non-lues")]
        [ProducesResponseType(typeof(List<AlerteResponse>), 200)]
        public async Task<IActionResult> GetNonLues(
            [FromQuery] string? typeAlerte = null,
            [FromQuery] string? niveau = null)
        {
            _logger.LogInformation("Récupération des alertes non lues");

            var query = new GetAlertesNonLuesQuery
            {
                TypeAlerte = typeAlerte,
                Niveau = niveau
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AlerteResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération de l'alerte {AlerteId}", id);

            var query = new GetAlerteByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            var response = result.ToResponse();
            return Ok(response);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(AlertesStatsDto), 200)]
        public async Task<IActionResult> GetStats(
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération des statistiques des alertes");

            var query = new GetAlertesStatsQuery
            {
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ============================================
        // COMMANDS
        // ============================================

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        [ProducesResponseType(typeof(AlerteResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateAlerteRequest request)
        {
            _logger.LogInformation("Création d'une nouvelle alerte");

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AlerteResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAlerteRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour de l'alerte {AlerteId}", id);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return Ok(response);
        }

        [HttpPost("marquer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> MarquerAlertes([FromBody] MarquerAlertesRequest request)
        {
            _logger.LogInformation("Marquage des alertes comme lues");

            var command = new MarquerAlertesCommand
            {
                AlerteIds = request.AlerteIds ?? new List<int>(),
                UtilisateurId = request.UtilisateurId,
                MarquerToutes = request.MarquerToutes
            };

            await _mediator.Send(command);
            return NoContent();
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression de l'alerte {AlerteId}", id);

            var command = new DeleteAlerteCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}