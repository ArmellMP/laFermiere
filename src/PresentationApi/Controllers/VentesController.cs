using PresentationApi.Security;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PresentationApi.Models.Requests.Ventes;
using PresentationApi.Models.Responses;
using Application.Features.Ventes.Queries.GetAllVentes;
using Application.Features.Ventes.Queries.GetVenteById;
using Application.Features.Ventes.Queries.GetVentesByBande;
using Application.Features.Ventes.Queries.GetVentesByClient;
using Application.Features.Ventes.Queries.GetVentesStats;
using Application.Features.Ventes.Commands.CreateVente;
using Application.Features.Ventes.Commands.UpdateVente;
using Application.Features.Ventes.Commands.DeleteVente;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    [Authorize(Roles = AppRoles.AdminComptable)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VentesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VentesController> _logger;

        public VentesController(
            IMediator mediator,
            ILogger<VentesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        [HttpGet]
        [ProducesResponseType(typeof(List<VenteResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? bandeId = null,
            [FromQuery] int? clientId = null,
            [FromQuery] string? typeVente = null,
            [FromQuery] bool? estPaye = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération de la liste des ventes");

            var query = new GetAllVentesQuery
            {
                BandeId = bandeId,
                ClientId = clientId,
                TypeVente = typeVente,
                EstPaye = estPaye,
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VenteResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération de la vente {VenteId}", id);

            var query = new GetVenteByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            var response = result.ToResponse();
            return Ok(response);
        }

        [HttpGet("bande/{bandeId}")]
        [ProducesResponseType(typeof(List<VenteResponse>), 200)]
        public async Task<IActionResult> GetByBande(int bandeId)
        {
            _logger.LogInformation("Récupération des ventes de la bande {BandeId}", bandeId);

            var query = new GetVentesByBandeQuery { BandeId = bandeId };
            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(List<VenteResponse>), 200)]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            _logger.LogInformation("Récupération des ventes du client {ClientId}", clientId);

            var query = new GetVentesByClientQuery { ClientId = clientId };
            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(VentesStatsDto), 200)]
        public async Task<IActionResult> GetStats(
            [FromQuery] int? bandeId = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération des statistiques des ventes");

            var query = new GetVentesStatsQuery
            {
                BandeId = bandeId,
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ============================================
        // COMMANDS
        // ============================================

        [HttpPost]
        [ProducesResponseType(typeof(VenteResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateVenteRequest request)
        {
            _logger.LogInformation("Création d'une nouvelle vente");

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(VenteResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVenteRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour de la vente {VenteId}", id);

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
            _logger.LogInformation("Suppression de la vente {VenteId}", id);

            var command = new DeleteVenteCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}