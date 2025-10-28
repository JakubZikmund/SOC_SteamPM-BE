using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Constants;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.GameSearch;
using SOC_SteamPM_BE.Utils;

namespace SOC_SteamPM_BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameSearchController : ControllerBase
{
    private readonly IGameSearchService _gameSearchService;
    private readonly ILogger<GameSearchController> _logger;

    public GameSearchController(IGameSearchService gameSearchService, ILogger<GameSearchController> logger)
    {
        _gameSearchService = gameSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Searches for Steam games by name
    /// </summary>
    /// <param name="search">Search term (1-200 characters)</param>
    /// <returns>List of matching games (max 10 results)</returns>
    /// <response code="200">Returns matching games</response>
    /// <response code="400">If the search parameter is invalid</response>
    [HttpGet("searchGame")]
    public IActionResult SearchGames(
        [FromQuery]
        [StringLength(ValidationConstants.MaxSearchLength, MinimumLength = ValidationConstants.MinSearchLength,
            ErrorMessage = ValidationConstants.SearchErrorMessage)]
        string? search)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid search parameters: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(new 
            { 
                error = "Validation Failed", 
                message = "Invalid search parameters.",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }
        
        try
        {
            // If no search term provided, return empty results
            if (string.IsNullOrWhiteSpace(search))
            {
                return Ok(new { games = new List<SearchGameModel>(), totalCount = 0, searchTerm = "" });
            }

            // Sanitize input - trim and remove excess whitespace
            var sanitizedSearch = search.Trim();
            
            // Search via facade service
            var games = _gameSearchService.SearchGamesByName(sanitizedSearch);
            
            _logger.LogInformation("Search for '{SearchTerm}' returned {Count} results", sanitizedSearch, games.Count);
            
            return Ok(new { 
                games, 
                totalCount = games.Count, 
                searchTerm = sanitizedSearch,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting games");
            return StatusCode(500, new { 
                error = "Internal Server Error", 
                message = "An unexpected error occurred" 
            });
        }
    }
}
