using PresentationApi.Security;
using Microsoft.AspNetCore.Authorization;
// PresentationApi/Controllers/AchatsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PresentationApi.Models.Requests.Achats;
using PresentationApi.Models.Responses;
using Application.Features.Achats.Queries.GetAllAchats;
using Application.Features.Achats.Queries.GetAchatById;
using Application.Features.Achats.Queries.GetAchatsByBande;
using Application.Features.Achats.Queries.GetAchatsByFournisseur;
using Application.Features.Achats.Queries.GetAchatsStats;
using Application.Features.Achats.Commands.CreateAchat;
using Application.Features.Achats.Commands.UpdateAchat;
using Application.Features.Achats.Commands.DeleteAchat;

namespace PresentationApi.Controllers
{
    [Authorize(Roles = AppRoles.AdminComptableFermier)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AchatsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<AchatsController> _logger;

        public AchatsController(
            IMediator mediator,
            IMapper mapper,
            ILogger<AchatsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================
        // QUERIES
        // ============================================

        [HttpGet]
        [ProducesResponseType(typeof(List<AchatResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? bandeId = null,
            [FromQuery] int? fournisseurId = null,
            [FromQuery] string? typeAchat = null,
            [FromQuery] bool? onlyNonRecus = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération de la liste des achats");

            var query = new GetAllAchatsQuery
            {
                BandeId = bandeId,
                FournisseurId = fournisseurId,
                TypeAchat = typeAchat,
                OnlyNonRecus = onlyNonRecus,
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            var response = _mapper.Map<List<AchatResponse>>(result);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AchatResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération de l'achat {AchatId}", id);

            var query = new GetAchatByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            var response = _mapper.Map<AchatResponse>(result);
            return Ok(response);
        }

        [HttpGet("bande/{bandeId}")]
        [ProducesResponseType(typeof(List<AchatResponse>), 200)]
        public async Task<IActionResult> GetByBande(int bandeId)
        {
            _logger.LogInformation("Récupération des achats de la bande {BandeId}", bandeId);

            var query = new GetAchatsByBandeQuery { BandeId = bandeId };
            var result = await _mediator.Send(query);
            var response = _mapper.Map<List<AchatResponse>>(result);
            return Ok(response);
        }

        [HttpGet("fournisseur/{fournisseurId}")]
        [ProducesResponseType(typeof(List<AchatResponse>), 200)]
        public async Task<IActionResult> GetByFournisseur(int fournisseurId)
        {
            _logger.LogInformation("Récupération des achats du fournisseur {FournisseurId}", fournisseurId);

            var query = new GetAchatsByFournisseurQuery { FournisseurId = fournisseurId };
            var result = await _mediator.Send(query);
            var response = _mapper.Map<List<AchatResponse>>(result);
            return Ok(response);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(AchatsStatsDto), 200)]
        public async Task<IActionResult> GetStats(
            [FromQuery] int? bandeId = null,
            [FromQuery] int? fournisseurId = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            _logger.LogInformation("Récupération des statistiques des achats");

            var query = new GetAchatsStatsQuery
            {
                BandeId = bandeId,
                FournisseurId = fournisseurId,
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ============================================
        // COMMANDS
        // ============================================

        [Authorize(Roles = AppRoles.AdminComptable)]
        [HttpPost]
        [ProducesResponseType(typeof(AchatResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateAchatRequest request)
        {
            _logger.LogInformation("Création d'un nouvel achat");

            var command = _mapper.Map<CreateAchatCommand>(request);
            var result = await _mediator.Send(command);

            var response = _mapper.Map<AchatResponse>(result);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [Authorize(Roles = AppRoles.AdminComptable)]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AchatResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAchatRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour de l'achat {AchatId}", id);

            var command = _mapper.Map<UpdateAchatCommand>(request);
            var result = await _mediator.Send(command);

            var response = _mapper.Map<AchatResponse>(result);
            return Ok(response);
        }

        [Authorize(Roles = AppRoles.AdminComptable)]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression de l'achat {AchatId}", id);

            var command = new DeleteAchatCommand { Id = id };
            await _mediator.Send(command);

            return NoContent();
        }
    }
}