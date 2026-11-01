using PresentationApi.Security;
using Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PresentationApi.Models.Requests.Utilisateurs;
using PresentationApi.Models.Responses;
using Application.Features.Utilisateurs.Queries.GetAllUtilisateurs;
using Application.Features.Utilisateurs.Queries.GetUtilisateurById;
using Application.Features.Utilisateurs.Queries.AuthenticateUser;
using Application.Features.Utilisateurs.Commands.CreateUtilisateur;
using Application.Features.Utilisateurs.Commands.UpdateUtilisateur;
using Application.Features.Utilisateurs.Commands.DeleteUtilisateur;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UtilisateursController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UtilisateursController> _logger;

        public UtilisateursController(
            IMediator mediator,
            ILogger<UtilisateursController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        [HttpGet]
        [ProducesResponseType(typeof(List<UtilisateurResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? role = null,
            [FromQuery] bool? onlyActifs = null,
            [FromQuery] string? searchTerm = null)
        {
            _logger.LogInformation("Récupération de la liste des utilisateurs");

            var query = new GetAllUtilisateursQuery
            {
                Role = role,
                OnlyActifs = onlyActifs,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UtilisateurResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération de l'utilisateur {UtilisateurId}", id);

            var query = new GetUtilisateurByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            var response = result.ToResponse();
            return Ok(response);
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            _logger.LogInformation("Tentative d'authentification pour {Email}", request.Email);

            try
            {
                var query = new AuthenticateUserQuery 
                { 
                    Email = request.Email, 
                    Password = request.Password 
                };
                
                var result = await _mediator.Send(query);
                
                var response = new AuthResponse
                {
                    Success = true,
                    Message = "Authentification réussie",
                    Utilisateur = result.Utilisateur.ToResponse(),
                    Token = result.Token,
                    Expiration = result.Expiration
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ============================================
        // COMMANDS
        // ============================================

        [HttpPost]
        [ProducesResponseType(typeof(UtilisateurResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateUtilisateurRequest request)
        {
            _logger.LogInformation("Création d'un nouvel utilisateur: {Email}", request.Email);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UtilisateurResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUtilisateurRequest request)
        {
            if (id != request.Id)
            {
                // Exception plutôt que BadRequest("texte") : ExceptionMiddleware renvoie le format commun ErrorResponse (400)
                throw new BusinessRuleException("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour de l'utilisateur {UtilisateurId}", id);

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
            _logger.LogInformation("Suppression de l'utilisateur {UtilisateurId}", id);

            var command = new DeleteUtilisateurCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}