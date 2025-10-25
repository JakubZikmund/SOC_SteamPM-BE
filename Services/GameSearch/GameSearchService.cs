using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.GameSearch;

public interface IGameSearchService
{
    List<SearchGameModel> SearchGamesByName(string searchTerm);
}

public class GameSearchService : IGameSearchService
{
    private readonly IEngineDataManager _dataManager;
    
    private const int MAX_RESULTS = 10;

    public GameSearchService(IEngineDataManager dataManager)
    {
        _dataManager = dataManager;
    }
    
    public List<SearchGameModel> SearchGamesByName(string searchTerm)
    {
        
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<SearchGameModel>();

        var results = new List<SearchGameModel>();
        var searchTermLower = searchTerm.ToLowerInvariant();

        var games = _dataManager.GetSteamGamesDict();
        
        // exact matches
        if (games.TryGetValue(searchTermLower, out var exactMatch))
        {
            results.Add(exactMatch);
        }

        // partial matching
        if (results.Count >= MAX_RESULTS) return results.Take(MAX_RESULTS).ToList();
        var partialMatches = games
            .Where(kvp => kvp.Key.Contains(searchTermLower) && kvp.Key != searchTermLower)
            .Select(kvp => kvp.Value)
            .Take(MAX_RESULTS - results.Count);

        results.AddRange(partialMatches);
        

        return results.ToList();
    }
}

