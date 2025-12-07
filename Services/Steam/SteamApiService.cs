using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.Steam;

public interface ISteamApiService
{
    Task<SteamGamesResponse> FetchAllGames();
    Task<SteamGameApiResponse> FetchGameById(int appId, string cc);
    Task<List<SteamPrice>> FetchGamePrices(int appId, string[] ccs);
    Task<List<WishlistItemResponseModel>> FetchWishlistItems(string steamId);
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
        var url = _steamSettings.AllGamesUrl.Replace("{API-KEY}", _steamSettings.ApiKey);
        _logger.LogInformation("Calling Steam API: {Url}", url);
        
        var response = await _httpClient.GetAsync(url);
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
        
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
        
            var jsonContent = await response.Content.ReadAsStringAsync();

            var dataProperty = ClearFirstLayerOfJsonData(jsonContent);

            if (!dataProperty.Value.TryGetProperty("success", out JsonElement successElement) || !successElement.GetBoolean())
            {
                _logger.LogWarning("Game with AppId {AppId} not found in Steam API", appId);
                throw new GameNotFoundException(appId);
            }
            
            if (!dataProperty.Value.TryGetProperty("data", out JsonElement dataElement))
            {
                _logger.LogError("Data property not found in Steam API response for AppId {AppId}", appId);
                throw new GameNotFoundException(appId, "Data property not found in Steam API response");
            }
            
            var gameData = dataElement.Deserialize<SteamGameApiResponse>();

            _logger.LogInformation("Successfully fetched game info for {AppId} from Steam API", appId);
            return gameData ?? new SteamGameApiResponse();
            
        }
        catch (GameNotFoundException)
        {
            throw;
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Tady to zachytí error pokud 429
                _logger.LogError(e, "Request for steam api reached maximum - rate limit exceeded.");
                throw new SteamApiRateLimitExceededException();
            }
            _logger.LogError(e, "Unexpected http request error while fetching game info for AppId {AppId}", appId);
            throw new Exception($"Failed to fetch game info for {appId} from Steam API", e);   
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while fetching game info for AppId {AppId}", appId);
            throw new Exception($"An error occurred while fetching game info for {appId}.", e);
        }
    }
    
    public async Task<List<SteamPrice>> FetchGamePrices(int appId, string[] ccs)
    {
        var priceList = new List<SteamPrice>();
        var baseUrl = _steamSettings.GamePriceInfo.Replace("{APPID}", appId.ToString());

        foreach (var cc in ccs)
        {
            try
            {
                var url = baseUrl.Replace("{CC}", cc);
                _logger.LogInformation($"Calling Steam API for currency/country: {cc}");
            
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                var dataProperty = ClearFirstLayerOfJsonData(jsonContent);

                if (!dataProperty.Value.TryGetProperty("success", out JsonElement successElement) || !successElement.GetBoolean())
                {
                    // Determines if the game is not available in a particular country or the appid is incorrect. In US is everything supported (at least I hope) and the cc code of US is first,
                    // so if the game is not available in US, it is probably not available in any other country -> throw exception.
                    if (cc.Equals("us"))
                    {
                        _logger.LogWarning("Game with AppId {AppId} not found in Steam API", appId);
                        throw new GameNotFoundException(appId);    
                    }
                    _logger.LogWarning("Game is not available in country {Currency}. Skipping...", cc);
                    continue;
                }
            
                if (!dataProperty.Value.TryGetProperty("data", out JsonElement dataElement))
                    throw new Exception("Data property not found in Steam API response");
                
                if (dataElement.ValueKind == JsonValueKind.Array || !dataElement.TryGetProperty("price_overview", out JsonElement priceElement))
                {
                    _logger.LogWarning("Game is not available in country {Currency}. Skipping... (price_overview not found)", cc);
                    continue;   
                }
                    
                var price = priceElement.Deserialize<SteamPrice>();

                price.Currency = cc switch
                {
                    "ar" => "USD-LATAM",
                    "dz" => "USD-CIS",
                    "np" => "USD-SASIA",
                    "am" => "USD-MENA",
                    _ => price.Currency
                };

                priceList.Add(price);
            }
            catch (GameNotFoundException)
            {
                throw;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError(e, "Request for steam api reached maximum - rate limit exceeded.");
                    throw new SteamApiRateLimitExceededException();
                }
                _logger.LogError(e, "Unexpected http request error while fetching game info for AppId {AppId}", appId);
                throw new Exception($"Failed to fetch game info for {appId} from Steam API", e);   
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while fetching game prices for AppId {AppId} in currency {Currency}", appId, cc);
                throw new Exception($"An error occurred while fetching game prices for currency {cc}.", e);
            }
        }
        
        return priceList;
    }

    public async Task<List<WishlistItemResponseModel>> FetchWishlistItems(string steamId)
    {
        try
        {
            var baseUrl = _steamSettings.WishlistUrl.Replace("{API-KEY}", _steamSettings.ApiKey).Replace("{STEAMID}", steamId);
            
            _logger.LogInformation($"Calling Steam API for wishlist with steam ID: {steamId}");
            
            var response = await _httpClient.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();
                
            var jsonContent = await response.Content.ReadAsStringAsync();
                
            var dataProperty = ClearFirstLayerOfJsonData(jsonContent);

            if (dataProperty.Value.ValueKind == JsonValueKind.Object && !dataProperty.Value.EnumerateObject().Any())
            {
                throw new WishlistNotFoundException(); 
            }
            
            if (!dataProperty.Value.TryGetProperty("items", out JsonElement dataElement))
                throw new Exception("Data property not found in Steam API response");
                
            var items = dataElement.Deserialize<List<WishlistItemResponseModel>>();
            
            items?.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            
            return items ?? new List<WishlistItemResponseModel>();
        }
        catch (WishlistNotFoundException)
        {
            throw;
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogError(e, "Request for steam api reached maximum - rate limit exceeded.");
                throw new SteamApiRateLimitExceededException();
            }
            _logger.LogError(e, "Unexpected http request error while fetching wishlist for steam ID {SteamId}", steamId);
            throw new Exception($"Failed to fetch wishlist for steam ID {steamId} from Steam API", e);   
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while fetching wishlist for steam ID {steamId}", steamId);
            throw new Exception($"An error occurred while fetching wishlist for steam ID {steamId}.", e);
        }
    }
    
    private static JsonProperty ClearFirstLayerOfJsonData(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;
        return root.EnumerateObject().FirstOrDefault();
    }
}