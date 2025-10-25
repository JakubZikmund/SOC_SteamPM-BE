using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.GameSearch;

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

    [HttpGet("searchGame")]
    public IActionResult SearchGames([FromQuery] string search)
    {
        try
        {
            // If no search term provided, return empty results
            if (string.IsNullOrWhiteSpace(search))
            {
                return Ok(new { games = new List<SearchGameModel>(), totalCount = 0, searchTerm = "" });
            }

            // Search via facade service
            var games = _gameSearchService.SearchGamesByName(search);
            
            _logger.LogInformation("Search for '{SearchTerm}' returned {Count} results", search, games.Count);
            
            return Ok(new { 
                games, 
                totalCount = games.Count, 
                searchTerm = search,
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
