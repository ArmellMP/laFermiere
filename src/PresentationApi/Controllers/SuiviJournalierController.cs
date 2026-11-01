using PresentationApi.Security;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PresentationApi.Models.Requests.Suivis;
using PresentationApi.Models.Responses;
using Application.Features.SuivisJournaliers.Queries.GetAllSuivisByBandeId;
using Application.Features.SuivisJournaliers.Queries.GetSuiviById;
using Application.Features.SuivisJournaliers.Queries.GetSuiviStats;
using Application.Features.SuivisJournaliers.Commands.CreateSuivi;
using Application.Features.SuivisJournaliers.Commands.UpdateSuivi;
using Application.Features.SuivisJournaliers.Commands.DeleteSuivi;
using PresentationApi.Mappings;

namespace PresentationApi.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des suivis journaliers.
    /// Permet d'enregistrer et de consulter les données quotidiennes des bandes.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SuiviJournalierController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SuiviJournalierController> _logger;

        public SuiviJournalierController(
            IMediator mediator,
            ILogger<SuiviJournalierController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ============================================
        // QUERIES (LECTURE)
        // ============================================

        /// <summary>
        /// Récupère tous les suivis d'une bande spécifique.
        /// </summary>
        /// <param name="bandeId">ID de la bande</param>
        /// <param name="dateDebut">Date de début (optionnel)</param>
        /// <param name="dateFin">Date de fin (optionnel)</param>
        /// <param name="onlyWithSoins">Uniquement les suivis avec soins</param>
        /// <param name="onlyWithMortalites">Uniquement les suivis avec mortalités</param>
        /// <returns>Liste des suivis de la bande</returns>
        [HttpGet("bande/{bandeId}")]
        [ProducesResponseType(typeof(List<SuiviResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByBandeId(
            int bandeId,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null,
            [FromQuery] bool onlyWithSoins = false,
            [FromQuery] bool onlyWithMortalites = false)
        {
            _logger.LogInformation("Récupération des suivis pour la bande {BandeId}", bandeId);

            var query = new GetAllSuivisByBandeIdQuery
            {
                BandeId = bandeId,
                DateDebut = dateDebut,
                DateFin = dateFin,
                OnlyWithSoins = onlyWithSoins,
                OnlyWithMortalites = onlyWithMortalites
            };

            var result = await _mediator.Send(query);
            var response = result.ToResponses();

            return Ok(response);
        }

        /// <summary>
        /// Récupère un suivi par son ID.
        /// </summary>
        /// <param name="id">ID du suivi</param>
        /// <returns>Le suivi correspondant</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuiviResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération du suivi {SuiviId}", id);

            var query = new GetSuiviByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Récupère les statistiques des suivis pour une bande ou une période.
        /// </summary>
        /// <param name="bandeId">ID de la bande (optionnel)</param>
        /// <param name="dateDebut">Date de début (optionnel)</param>
        /// <param name="dateFin">Date de fin (optionnel)</param>
        /// <returns>Statistiques des suivis</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(SuiviStatsDto), 200)]
        public async Task<IActionResult> GetStats(
            [FromQuery] int? bandeId = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération des statistiques des suivis");

            var query = new GetSuiviStatsQuery
            {
                BandeId = bandeId,
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ============================================
        // COMMANDS (ÉCRITURE)
        // ============================================

        /// <summary>
        /// Crée un nouveau suivi journalier.
        /// </summary>
        /// <param name="request">Données du suivi</param>
        /// <returns>Le suivi créé</returns>
        [Authorize(Roles = AppRoles.AdminFermierVeterinaire)]
        [HttpPost]
        [ProducesResponseType(typeof(SuiviResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateSuiviRequest request)
        {
            _logger.LogInformation(
                "Création d'un suivi pour la bande {BandeId}, jour {Jour}", 
                request.BandeId, request.Jour);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Met à jour un suivi existant.
        /// </summary>
        /// <param name="id">ID du suivi</param>
        /// <param name="request">Données de mise à jour</param>
        /// <returns>Le suivi mis à jour</returns>
        [Authorize(Roles = AppRoles.AdminFermierVeterinaire)]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SuiviResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSuiviRequest request)
        {
            if (id != request.Id)
            {
                // Exception plutôt que BadRequest("texte") : ExceptionMiddleware renvoie le format commun ErrorResponse (400)
                throw new BusinessRuleException("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour du suivi {SuiviId}", id);

            var command = request.ToCommand();
            var result = await _mediator.Send(command);

            var response = result.ToResponse();
            return Ok(response);
        }

        /// <summary>
        /// Supprime un suivi. Attention: Cette opération est irréversible.
        /// </summary>
        /// <param name="id">ID du suivi</param>
        /// <returns>Confirmation de suppression</returns>
        [Authorize(Roles = AppRoles.AdminFermierVeterinaire)]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression du suivi {SuiviId}", id);

            var command = new DeleteSuiviCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}