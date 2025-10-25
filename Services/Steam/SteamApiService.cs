using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.Steam;

public interface ISteamApiService
{
    Task<SteamGamesResponse> FetchAllGames();
    //TBD: fetch game by id
    Task<object> FetchGameById(int appId);
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
        _logger.LogInformation("Calling Steam API: {Url}", _steamSettings.BaseUrl);
        
        var response = await _httpClient.GetAsync(_steamSettings.BaseUrl);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var gameData = JsonSerializer.Deserialize<SteamGamesResponse>(jsonContent);
        
        _logger.LogInformation("Successfully fetched {GameCount} games from Steam API", 
            gameData?.Applist?.Apps?.Count ?? 0);

        return gameData ?? new SteamGamesResponse();
    }
    
    public async Task<object> FetchGameById(int appId)
    {
        throw new NotImplementedException();
    }
}

