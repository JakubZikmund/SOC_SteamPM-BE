using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Utils;

public static class DataFactory
{
    public static Dictionary<string, SearchGameModel> DictionaryFromSteamGames(SteamGamesResponse? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var games = data.Applist.Apps;

        if (games == null)
            throw new Exception("No games found!");
        
        var gamesByNameLowercase = new Dictionary<string, SearchGameModel>();
        foreach (var game in games)
        {
            if (string.IsNullOrWhiteSpace(game.Name))
                continue;

            var nameLower = game.Name.ToLowerInvariant();
            gamesByNameLowercase[nameLower] = game;
        }
        return gamesByNameLowercase;
    }
}