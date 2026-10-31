using PresentationApi.Security;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PresentationApi.Models.Requests.Clients;
using PresentationApi.Models.Responses;
using Application.Features.Clients.Queries.GetAllClients;
using Application.Features.Clients.Queries.GetClientById;
using Application.Features.Clients.Queries.GetClientsByNom;
using Application.Features.Clients.Queries.GetClientsByType;
using Application.Features.Clients.Commands.CreateClient;
using Application.Features.Clients.Commands.UpdateClient;
using Application.Features.Clients.Commands.DeleteClient;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des clients.
    /// </summary>
    [Authorize(Roles = AppRoles.AdminComptable)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(
            IMediator mediator,
            ILogger<ClientsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        /// <summary>
        /// Récupère tous les clients avec filtres optionnels.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ClientResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? typeClient = null,
            [FromQuery] bool? onlyActifs = null,
            [FromQuery] string? searchTerm = null)
        {
            _logger.LogInformation("Récupération de la liste des clients");

            var query = new GetAllClientsQuery
            {
                TypeClient = typeClient,
                OnlyActifs = onlyActifs,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();

            return Ok(response);
        }

        /// <summary>
        /// Récupère un client par son ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClientResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération du client {ClientId}", id);

            var query = new GetClientByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Recherche des clients par nom.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<ClientResponse>), 200)]
        public async Task<IActionResult> Search(
            [FromQuery] string term,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation("Recherche de clients avec le terme '{Term}'", term);

            var query = new GetClientsByNomQuery { SearchTerm = term, Limit = limit };
            var result = await _mediator.Send(query);

            var response = result.ToResponses();
            return Ok(response);
        }

        /// <summary>
        /// Récupère les clients par type.
        /// </summary>
        [HttpGet("type/{typeClient}")]
        [ProducesResponseType(typeof(List<ClientResponse>), 200)]
        public async Task<IActionResult> GetByType(string typeClient, [FromQuery] bool onlyActifs = true)
        {
            _logger.LogInformation("Récupération des clients de type {TypeClient}", typeClient);

            var query = new GetClientsByTypeQuery
            {
                TypeClient = typeClient,
                OnlyActifs = onlyActifs
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();

            return Ok(response);
        }

        // ============================================
        // COMMANDS
        // ============================================

        /// <summary>
        /// Crée un nouveau client.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClientResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
        {
            _logger.LogInformation("Création d'un nouveau client: {Nom}", request.Nom);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Met à jour un client existant.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ClientResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour du client {ClientId}", id);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Supprime un client.
        /// ⚠️ Attention: Un client avec des ventes ne peut pas être supprimé.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression du client {ClientId}", id);

            var command = new DeleteClientCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}