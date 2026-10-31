using PresentationApi.Security;
using Microsoft.AspNetCore.Authorization;
// PresentationApi/Controllers/StocksController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PresentationApi.Models.Requests.Stocks;
using PresentationApi.Models.Responses;
using Application.Features.Stocks.Queries.GetAllStocks;
using Application.Features.Stocks.Queries.GetStockById;
using Application.Features.Stocks.Queries.GetStocksByBande;
using Application.Features.Stocks.Queries.GetStocksEnAlerte;
using Application.Features.Stocks.Queries.GetStocksPerimes;
using Application.Features.Stocks.Commands.CreateStock;
using Application.Features.Stocks.Commands.UpdateStock;
using Application.Features.Stocks.Commands.DeleteStock;
using Application.Features.Stocks.Commands.ConsommerStock;

namespace PresentationApi.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des stocks.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StocksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<StocksController> _logger;

        public StocksController(
            IMediator mediator,
            IMapper mapper,
            ILogger<StocksController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les stocks avec filtres optionnels.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<StockResponse>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? type = null,
            [FromQuery] int? fournisseurId = null,
            [FromQuery] bool onlyEnAlerte = false,
            [FromQuery] bool onlyNonPerimes = false)
        {
            _logger.LogInformation("Récupération des stocks");
            
            var query = new GetAllStocksQuery
            {
                Type = type,
                FournisseurId = fournisseurId,
                OnlyEnAlerte = onlyEnAlerte,
                OnlyNonPerimes = onlyNonPerimes
            };
            
            var result = await _mediator.Send(query);
            var response = _mapper.Map<List<StockResponse>>(result);
            
            return Ok(response);
        }

        /// <summary>
        /// Récupère un stock par son ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(StockResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Récupération du stock {StockId}", id);
            
            var query = new GetStockByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            var response = _mapper.Map<StockResponse>(result);
            return Ok(response);
        }

        /// <summary>
        /// Récupère les stocks d'une bande spécifique.
        /// </summary>
        [HttpGet("bande/{bandeId}")]
        [ProducesResponseType(typeof(List<StockResponse>), 200)]
        public async Task<IActionResult> GetByBande(int bandeId)
        {
            _logger.LogInformation("Récupération des stocks de la bande {BandeId}", bandeId);
            
            var query = new GetStocksByBandeQuery { BandeId = bandeId };
            var result = await _mediator.Send(query);
            
            var response = _mapper.Map<List<StockResponse>>(result);
            return Ok(response);
        }

        /// <summary>
        /// Récupère les stocks en alerte.
        /// </summary>
        [HttpGet("alerte")]
        [ProducesResponseType(typeof(List<StockResponse>), 200)]
        public async Task<IActionResult> GetEnAlerte([FromQuery] int? bandeId = null)
        {
            _logger.LogInformation("Récupération des stocks en alerte");
            
            var query = new GetStocksEnAlerteQuery { BandeId = bandeId };
            var result = await _mediator.Send(query);
            
            var response = _mapper.Map<List<StockResponse>>(result);
            return Ok(response);
        }

        /// <summary>
        /// Récupère les stocks périmés.
        /// </summary>
        [HttpGet("perimes")]
        [ProducesResponseType(typeof(List<StockResponse>), 200)]
        public async Task<IActionResult> GetPerimes([FromQuery] int? joursAlerte = null)
        {
            _logger.LogInformation("Récupération des stocks périmés");
            
            var query = new GetStocksPerimesQuery { JoursAlerte = joursAlerte };
            var result = await _mediator.Send(query);
            
            var response = _mapper.Map<List<StockResponse>>(result);
            return Ok(response);
        }

        /// <summary>
        /// Crée un nouveau stock.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpPost]
        [ProducesResponseType(typeof(StockResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateStockRequest request)
        {
            _logger.LogInformation("Création d'un nouveau stock: {Nom}", request.Nom);
            
            var command = _mapper.Map<CreateStockCommand>(request);
            var result = await _mediator.Send(command);
            
            var response = _mapper.Map<StockResponse>(result);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Met à jour un stock existant.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(StockResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStockRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Mise à jour du stock {StockId}", id);
            
            var command = _mapper.Map<UpdateStockCommand>(request);
            var result = await _mediator.Send(command);
            
            var response = _mapper.Map<StockResponse>(result);
            return Ok(response);
        }

        /// <summary>
        /// Consomme une quantité d'un stock.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpPost("{id}/consommer")]
        [ProducesResponseType(typeof(StockResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Consommer(int id, [FromBody] ConsommerStockRequest request)
        {
            if (id != request.StockId)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans la requête.");
            }

            _logger.LogInformation("Consommation du stock {StockId}: {Quantite}", id, request.QuantiteConsommee);
            
            var command = _mapper.Map<ConsommerStockCommand>(request);
            var result = await _mediator.Send(command);
            
            var response = _mapper.Map<StockResponse>(result);
            return Ok(response);
        }

        /// <summary>
        /// Supprime un stock.
        /// </summary>
        [Authorize(Roles = AppRoles.AdminFermier)]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Suppression du stock {StockId}", id);
            
            var command = new DeleteStockCommand { Id = id };
            await _mediator.Send(command);
            
            return NoContent();
        }
    }
}