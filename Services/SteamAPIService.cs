using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services;

public interface ISteamAPIService
{
    Task<SteamGameResponse> FetchAllGames();
    //TBD: fetch game by id
    Task<object> FetchGameById(int appId);
}

public class SteamAPIService : ISteamAPIService
{
    private readonly HttpClient _httpClient;
    private readonly SteamApiSettings _steamSettings;
    private readonly ILogger<SteamAPIService> _logger;

    public SteamAPIService(HttpClient httpClient, IOptions<SteamApiSettings> steamSettings, ILogger<SteamAPIService> logger)
    {
        _httpClient = httpClient;
        _steamSettings = steamSettings.Value;
        _logger = logger;
    }


    public async Task<SteamGameResponse> FetchAllGames()
    {
        _logger.LogInformation("Calling Steam API: {Url}", _steamSettings.BaseUrl);
        
        var response = await _httpClient.GetAsync(_steamSettings.BaseUrl);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var gameData = JsonSerializer.Deserialize<SteamGameResponse>(jsonContent);
        
        _logger.LogInformation("Successfully fetched {GameCount} games from Steam API", 
            gameData?.Applist?.Apps?.Count ?? 0);

        return gameData ?? new SteamGameResponse();
    }
    
    public async Task<object> FetchGameById(int appId)
    {
        throw new NotImplementedException();
    }
}