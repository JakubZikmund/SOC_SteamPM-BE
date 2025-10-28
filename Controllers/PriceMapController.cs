using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Services.PriceMap;
using SOC_SteamPM_BE.Services.Steam;

namespace SOC_SteamPM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceMapController : ControllerBase
    {
        private readonly IPriceMapService _priceMapService;
        private readonly ILogger<PriceMapController> _logger;

        public PriceMapController(ILogger<PriceMapController> logger, IPriceMapService priceMapService)
        {
            _priceMapService = priceMapService;
            _logger = logger;
        }

        [HttpGet("game/{appId:int}")]
        public async Task<IActionResult> GetGameById(int appId, [FromQuery] string currency = "EUR")
        {
            try
            {
                var data = await _priceMapService.GetGameInfoAndPrices(appId, currency);
                return Ok(data);
            }
            catch (GameNotFoundException ex)
            {
                _logger.LogWarning("Game not found: {Message}", ex.Message);
                return NotFound(new 
                { 
                    error = "Game Not Found", 
                    message = $"Steam game with AppId {appId} was not found.",
                    appId = ex.AppId
                });
            }
            catch (InvalidCurrencyException ex)
            {
                _logger.LogWarning("Invalid currency code: {Message}", ex.Message);
                return BadRequest(new 
                { 
                    error = "Invalid Currency", 
                    message = $"Invalid currency code: '{ex.CurrencyCode}'. Please provide a valid currency code (e.g., EUR, USD, CZK).",
                    currencyCode = ex.CurrencyCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting game by ID {AppId} with currency {Currency}", appId, currency);
                return StatusCode(500, new 
                { 
                    error = "Internal Server Error", 
                    message = "An unexpected error occurred while processing the request." 
                });
            }
        }
        
    }
}
