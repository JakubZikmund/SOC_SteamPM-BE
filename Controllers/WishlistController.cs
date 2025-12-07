using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Constants;
using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Services.Wishlist;
using SOC_SteamPM_BE.Utils;

namespace SOC_SteamPM_BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated wishlist from steam ID
    /// </summary>
    /// <param name="steamId">The Steam user's ID</param>
    /// <param name="page">Wishlist page (Default is 1)</param>
    /// <returns>Wishlist games, current page and wishlist size</returns>
    /// <response code="200">Returns wishlist games, current page and wishlist size</response>
    /// <response code="400">If the request parameters are invalid</response>
    /// <response code="404">If the wishlist is not found</response>
    [HttpGet("wishlist/{page:int}")]
        public async Task<IActionResult> GetWishlist(
            [FromQuery, Required] string steamId,
            [Range(ValidationConstants.MinPage, ValidationConstants.MaxPage, ErrorMessage = ValidationConstants.PageErrorMessage)] int page = 1
        )
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

            if (!SteamIdValidation.IsSteamIdValid(steamId))
            {
                return BadRequest(new 
                { 
                    error = "Steam ID is not valid", 
                    message = "Invalid request parameters.",
                });
            }
            
            try
            {
                var data = await _wishlistService.GetPaginatedWishlistAsync(steamId, page);
                return Ok(data);
            }
            catch (WishlistNotFoundException ex)
            {
                _logger.LogWarning("Wishlist not found: {Message}", ex.Message);
                return NotFound(new
                {
                    error = "Wishlist Not Found",
                    message = $"Steam wishlist with steam ID {steamId} was not found."
                });
            }
            catch (SteamApiRateLimitExceededException)
            {
                return StatusCode(429, new
                {
                    error = "Steam API rate limit exceed",
                    message = "Steam API rate limit exceeded. Please try again later."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when getting wishlist by steam ID {steamId}", steamId);
                return StatusCode(500, new 
                { 
                    error = "Internal Server Error", 
                    message = "An unexpected error occurred while processing the request." 
                });
            }
        }
}