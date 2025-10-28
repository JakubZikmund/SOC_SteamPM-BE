using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.Currencies;

public interface ICurrencyApiService
{
    public Task<CurrencyModel> FetchCurrency(string currency);
}

public class CurrencyApiService : ICurrencyApiService
{
    private readonly ILogger<CurrencyService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    
    
    public CurrencyApiService(ILogger<CurrencyService> logger, HttpClient httpClient, IOptions<CurrencySettings> currencySettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _baseUrl = currencySettings.Value.BaseUrl.Replace("{API-KEY}", currencySettings.Value.ApiKey);
    }

    public async Task<CurrencyModel> FetchCurrency(string currency)
    {
        try
        {
            _logger.LogInformation($"Calling Currency API to retrieve {currency} exchange rates.");
        
            var response = await _httpClient.GetAsync($"{_baseUrl}{currency}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var currencyData = JsonSerializer.Deserialize<CurrencyModel>(jsonContent);
            
            return currencyData ?? new CurrencyModel();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"An error occurred while fetching currency {currency}.", e);
        }
    }
}