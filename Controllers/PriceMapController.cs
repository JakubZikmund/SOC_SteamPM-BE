using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Services.PriceMap;
using SOC_SteamPM_BE.Services.Steam;
using SOC_SteamPM_BE.Utils;

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

        /// <summary>
        /// Retrieves game information and prices for a specific Steam AppId
        /// </summary>
        /// <param name="appId">The Steam Application ID (must be positive)</param>
        /// <param name="currency">Target currency code (3 uppercase letters, e.g., EUR, USD, CZK)</param>
        /// <returns>Game information with converted prices</returns>
        /// <response code="200">Returns the game information with prices</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="404">If the game is not found</response>
        [HttpGet("game/{appId:int}")]
        public async Task<IActionResult> GetGameById(
            [Range(ValidationConstants.MinAppId, ValidationConstants.MaxAppId, 
                ErrorMessage = ValidationConstants.AppIdErrorMessage)] 
            int appId, 
            [FromQuery]
            [RegularExpression(ValidationConstants.CurrencyPattern, 
                ErrorMessage = ValidationConstants.CurrencyErrorMessage)]
            [StringLength(ValidationConstants.CurrencyLength, MinimumLength = ValidationConstants.CurrencyLength)]
            string currency = "EUR")
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request parameters: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(new 
                { 
                    error = "Validation Failed", 
                    message = "Invalid request parameters.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            
            try
            {
                var data = await _priceMapService.GetGameInfoAndPrices(appId, currency.ToUpperInvariant());
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
