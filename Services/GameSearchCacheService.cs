using SOC_SteamPM_BE.Models;
using System.Collections.Concurrent;

namespace SOC_SteamPM_BE.Services;

public interface IGameCacheService
{
    void LoadGames(SteamGameResponse steamResponse);
    List<SearchGame> SearchGamesByName(string searchTerm);
    int GetGameCount();
    void ClearCache();
}

public class GameSearchCacheService : IGameCacheService
{
    // Thread-safe dictionary for fast lookups by AppId
    private readonly ConcurrentDictionary<int, SearchGame> _gamesById = new();
    
    private readonly int _maxResults = 10;
    
    // Regular dictionary for name-based searches (protected by lock since we rebuild it entirely)
    private readonly object _nameCacheLock = new();
    private Dictionary<string, List<SearchGame>> _gamesByNameLowercase = new();
    
    private readonly ILogger<GameSearchCacheService> _logger;

    public GameSearchCacheService(ILogger<GameSearchCacheService> logger)
    {
        _logger = logger;
    }

    
    public void LoadGames(SteamGameResponse steamResponse)
    {
        if (steamResponse?.Applist?.Apps == null)
        {
            _logger.LogWarning("Received null or empty steam response");
            return;
        }

        var games = steamResponse.Applist.Apps;
        _logger.LogInformation("Loading {Count} games into cache", games.Count);

        // Clear existing cache
        _gamesById.Clear();

        // Load games into ID-based cache (thread-safe)
        foreach (var game in games)
        {
            if (!string.IsNullOrWhiteSpace(game.Name))
            {
                _gamesById[game.AppId] = game;
            }
        }

        // Rebuild name-based search cache (requires lock)
        RebuildNameCache(games);

        _logger.LogInformation("Successfully loaded {Count} games into cache", _gamesById.Count);
    }

    public List<SearchGame> SearchGamesByName(string searchTerm)
    {
        
        
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<SearchGame>();

        var results = new List<SearchGame>();
        var searchTermLower = searchTerm.ToLowerInvariant();

        lock (_nameCacheLock)
        {
            // First, try exact matches
            if (_gamesByNameLowercase.TryGetValue(searchTermLower, out var exactMatches))
            {
                results.AddRange(exactMatches.Take(_maxResults));
            }

            // If we need more results, do partial matching
            if (results.Count >= _maxResults) return results.Take(_maxResults).ToList();
            var partialMatches = _gamesByNameLowercase
                .Where(kvp => kvp.Key.Contains(searchTermLower) && kvp.Key != searchTermLower)
                .SelectMany(kvp => kvp.Value)
                .Take(_maxResults - results.Count);

            results.AddRange(partialMatches);
        }

        return results.Take(_maxResults).ToList();
    }

    public void ClearCache()
    {
        _gamesById.Clear();
        
        lock (_nameCacheLock)
        {
            _gamesByNameLowercase.Clear();
        }
        
        _logger.LogInformation("Game cache cleared");
    }
    
    public int GetGameCount()
    {
        return _gamesById.Count;
    }

    private void RebuildNameCache(List<SearchGame> games)
    {
        lock (_nameCacheLock)
        {
            _gamesByNameLowercase.Clear();

            foreach (var game in games)
            {
                if (string.IsNullOrWhiteSpace(game.Name))
                    continue;

                var nameLower = game.Name.ToLowerInvariant();
                
                if (!_gamesByNameLowercase.ContainsKey(nameLower))
                {
                    _gamesByNameLowercase[nameLower] = new List<SearchGame>();
                }
                
                _gamesByNameLowercase[nameLower].Add(game);
            }

            _logger.LogDebug("Rebuilt name cache with {Count} unique names", _gamesByNameLowercase.Count);
        }
    }
}

