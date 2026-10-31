// PresentationApi/Controllers/BandesController.cs
using PresentationApi.Security;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Domain.Enums;
using PresentationApi.Models.Requests.Bandes;
using PresentationApi.Models.Responses;
using Application.Features.Bandes.Queries.GetAllBandes;
using Application.Features.Bandes.Queries.GetBandeById;
using Application.Features.Bandes.Queries.GetBandesByStatut;
using Application.Features.Bandes.Queries.GetBandeStats;
using Application.Features.Bandes.Commands.CreateBande;
using Application.Features.Bandes.Commands.UpdateBande;
using Application.Features.Bandes.Commands.CloturerBande;
using Application.Features.Bandes.Commands.DeleteBande;
using PresentationApi.Mappings;
using Application.Features.Bandes.Dtos;                        
namespace PresentationApi.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des bandes d'élevage.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BandesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BandesController> _logger;

        public BandesController(
            IMediator mediator,
            ILogger<BandesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Récupère toutes les bandes.
        /// </summary>
        /// <param name="includeDetails">Inclure les détails (mortalités, jours, etc.)</param>
        /// <returns>Liste des bandes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<BandeResponse>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDetails = false)
        {
            _logger.LogInformation("Récupération de toutes les bandes (IncludeDetails: {IncludeDetails})", includeDetails);
            
            var query = new GetAllBandesQuery { IncludeDetails = includeDetails };
            var result = await _mediator.Send(query);
            
            var response = result.ToResponses();
            return Ok(response);
        }

        /// <summary>
        /// Récupère une bande par son ID.
        /// </summary>
        /// <param name="id">ID de la bande</param>
        /// <returns>La bande correspondante</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BandeResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération de la bande {BandeId}", id);
            
            var query = new GetBandeByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Récupère les bandes par statut.
        /// </summary>
        /// <param name="statut">Statut (EnPreparation, EnCours, Cloturee, Archivee)</param>
        /// <returns>Liste des bandes filtrées</returns>
        [HttpGet("statut/{statut}")]
        [ProducesResponseType(typeof(List<BandeResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetByStatut(string statut)
        {
            _logger.LogInformation("Récupération des bandes avec statut {Statut}", statut);
            
            // Convertir string → enum
            if (!Enum.TryParse<StatutBande>(statut, true, out var statutEnum))
            {
                return BadRequest($"Statut invalide. Valeurs possibles: {string.Join(", ", Enum.GetNames<StatutBande>())}");
            }
            
            var query = new GetBandesByStatutQuery { Statut = statutEnum };
            var result = await _mediator.Send(query);
            
            var response = result.ToResponses();
            return Ok(response);
        }

        /// <summary>
        /// Crée une nouvelle bande.
        /// </summary>
        /// <param name="request">Données de création</param>
        /// <returns>La bande créée</returns>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpPost]
        [ProducesResponseType(typeof(BandeResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateBandeRequest request)
        {
            _logger.LogInformation("Création d'une nouvelle bande");
            
            var command = request.ToCommand();
            var result = await _mediator.Send(command);
            
            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Met à jour une bande existante.
        /// </summary>
        /// <param name="id">ID de la bande</param>
        /// <param name="request">Données de mise à jour</param>
        /// <returns>La bande mise à jour</returns>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BandeResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBandeRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps de la requête.");
            }

            _logger.LogInformation("Mise à jour de la bande {BandeId}", id);
            
            var command = request.ToCommand();
            var result = await _mediator.Send(command);
            
            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Clôture une bande.
        /// </summary>
        /// <param name="id">ID de la bande</param>
        /// <param name="request">Données de clôture</param>
        /// <returns>La bande clôturée</returns>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpPost("{id}/cloturer")]
        [ProducesResponseType(typeof(BandeResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Cloturer(int id, [FromBody] CloturerBandeRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps de la requête.");
            }

            _logger.LogInformation("Clôture de la bande {BandeId}", id);
            
            var command = request.ToCommand();
            var result = await _mediator.Send(command);
            
            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Supprime une bande.
        /// </summary>
        /// <param name="id">ID de la bande</param>
        /// <returns>Confirmation de suppression</returns>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression de la bande {BandeId}", id);
            
            var command = new DeleteBandeCommand { Id = id };
            await _mediator.Send(command);
            
            return NoContent();
        }

        /// <summary>
        /// Récupère les statistiques des bandes.
        /// </summary>
        /// <returns>Statistiques</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(BandeStatsDto), 200)] 
        public async Task<IActionResult> GetStats()
        {
            _logger.LogInformation("Récupération des statistiques des bandes");
            
           // Créer et envoyer la requête pour obtenir les statistiques
            var query = new GetBandeStatsQuery();
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
    }
}