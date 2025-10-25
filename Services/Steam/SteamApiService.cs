using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.Steam;

public interface ISteamApiService
{
    Task<SteamGamesResponse> FetchAllGames();
    Task<SteamGameApiResponse> FetchGameById(int appId, string cc);
}

public class SteamApiService : ISteamApiService
{
    private readonly HttpClient _httpClient;
    private readonly SteamApiSettings _steamSettings;
    private readonly ILogger<SteamApiService> _logger;

    public SteamApiService(HttpClient httpClient, IOptions<SteamApiSettings> steamSettings, ILogger<SteamApiService> logger)
    {
        _httpClient = httpClient;
        _steamSettings = steamSettings.Value;
        _logger = logger;
    }


    public async Task<SteamGamesResponse> FetchAllGames()
    {
        _logger.LogInformation("Calling Steam API: {Url}", _steamSettings.AllGamesUrl);
        
        var response = await _httpClient.GetAsync(_steamSettings.AllGamesUrl);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var gameData = JsonSerializer.Deserialize<SteamGamesResponse>(jsonContent);
        
        _logger.LogInformation("Successfully fetched {GameCount} games from Steam API", 
            gameData?.Applist?.Apps?.Count ?? 0);

        return gameData ?? new SteamGamesResponse();
    }
    
    public async Task<SteamGameApiResponse> FetchGameById(int appId, string cc)
    {
        try
        {
            var url = _steamSettings.GameAllInfo.Replace("{APPID}", appId.ToString()).Replace("{CC}", cc);;
            _logger.LogInformation("Calling Steam API: {Url}", url);
            Console.WriteLine(url);
        
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
        
            var jsonContent = await response.Content.ReadAsStringAsync();
            
            using (JsonDocument document = JsonDocument.Parse(jsonContent))
            {
                JsonElement root = document.RootElement;
    
                // Získat první (a jediný) property - což je to ID hry
                var gameProperty = root.EnumerateObject().FirstOrDefault();

                if (!gameProperty.Value.TryGetProperty("data", out JsonElement dataElement))
                    throw new Exception("Data property not found in Steam API response");
                var gameData = dataElement.Deserialize<SteamGameApiResponse>();

                _logger.LogInformation("Successfully fetched game info for {AppId} from Steam API", appId);
                return gameData ?? new SteamGameApiResponse();
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to fetch game info for {appId} from Steam API", e);
        }
    }
}