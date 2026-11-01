using PresentationApi.Security;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PresentationApi.Models.Requests.Fournisseurs;
using PresentationApi.Models.Responses;
using Application.Features.Fournisseurs.Queries.GetAllFournisseurs;
using Application.Features.Fournisseurs.Queries.GetFournisseurById;
using Application.Features.Fournisseurs.Queries.GetFournisseursByNom;
using Application.Features.Fournisseurs.Queries.GetFournisseursByType;
using Application.Features.Fournisseurs.Commands.CreateFournisseur;
using Application.Features.Fournisseurs.Commands.UpdateFournisseur;
using Application.Features.Fournisseurs.Commands.DeleteFournisseur;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    [Authorize(Roles = AppRoles.AdminComptable)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FournisseursController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FournisseursController> _logger;

        public FournisseursController(
            IMediator mediator,
            ILogger<FournisseursController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        [HttpGet]
        [ProducesResponseType(typeof(List<FournisseurResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? typeFournisseur = null,
            [FromQuery] bool? onlyActifs = null,
            [FromQuery] string? searchTerm = null)
        {
            _logger.LogInformation("Récupération de la liste des fournisseurs");

            var query = new GetAllFournisseursQuery
            {
                TypeFournisseur = typeFournisseur,
                OnlyActifs = onlyActifs,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FournisseurResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération du fournisseur {FournisseurId}", id);

            var query = new GetFournisseurByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            var response = result.ToResponse();
            return Ok(response);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(List<FournisseurResponse>), 200)]
        public async Task<IActionResult> Search(
            [FromQuery] string term,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation("Recherche de fournisseurs avec le terme '{Term}'", term);

            var query = new GetFournisseursByNomQuery { SearchTerm = term, Limit = limit };
            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("type/{typeFournisseur}")]
        [ProducesResponseType(typeof(List<FournisseurResponse>), 200)]
        public async Task<IActionResult> GetByType(string typeFournisseur, [FromQuery] bool onlyActifs = true)
        {
            _logger.LogInformation("Récupération des fournisseurs de type {TypeFournisseur}", typeFournisseur);

            var query = new GetFournisseursByTypeQuery
            {
                TypeFournisseur = typeFournisseur,
                OnlyActifs = onlyActifs
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        // ============================================
        // COMMANDS
        // ============================================

        [HttpPost]
        [ProducesResponseType(typeof(FournisseurResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateFournisseurRequest request)
        {
            _logger.LogInformation("Création d'un nouveau fournisseur: {Nom}", request.Nom);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FournisseurResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFournisseurRequest request)
        {
            if (id != request.Id)
            {
                // Exception plutôt que BadRequest("texte") : ExceptionMiddleware renvoie le format commun ErrorResponse (400)
                throw new BusinessRuleException("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour du fournisseur {FournisseurId}", id);

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
            _logger.LogInformation("Suppression du fournisseur {FournisseurId}", id);

            var command = new DeleteFournisseurCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}