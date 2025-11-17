using System.Text.Json.Serialization;

namespace SOC_SteamPM_BE.Models;

public class SearchGameModel
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class AppList
{
    [JsonPropertyName("apps")]
    public List<SearchGameModel> Apps { get; set; } = new();
}

public class SteamGamesResponse
{
    [JsonPropertyName("response")]
    public AppList Applist { get; set; } = new();
}

