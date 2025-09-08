using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Services;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameSearchController : ControllerBase
{
    private readonly IGameCacheService _gameCache;
    private readonly ILogger<GameSearchController> _logger;

    public GameSearchController(IGameCacheService gameCache, ILogger<GameSearchController> logger)
    {
        _gameCache = gameCache;
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
                return Ok(new { games = new List<SearchGame>(), totalCount = 0, searchTerm = "" });
            }

            // Use cache for fast search
            var games = _gameCache.SearchGamesByName(search);
            
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
    
    [HttpGet("gamesCount")]
    public IActionResult GetGamesCount()
    {
        return Ok(new { count = _gameCache.GetGameCount() });
    }
}
