using System.Text.Json.Serialization;

namespace SOC_SteamPM_BE.Models;

public class Game
{
    // TBD
}

public class SearchGame
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class AppList
{
    [JsonPropertyName("apps")]
    public List<SearchGame> Apps { get; set; } = new();
}

public class SteamGameResponse
{
    [JsonPropertyName("applist")]
    public AppList Applist { get; set; } = new();
}

