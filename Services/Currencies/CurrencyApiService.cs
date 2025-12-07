using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Exceptions;
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
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Invalid currency code: {Currency}", currency);
                throw new InvalidCurrencyException(currency);
            }

            if (!response.IsSuccessStatusCode)
            {
                var jsonErrorContent = await response.Content.ReadAsStringAsync();
                var errorData = JsonSerializer.Deserialize<CurrencyErrorModel>(jsonErrorContent);

                if (errorData is { Result: "error", ErrorType: "quota-reached" })
                {
                    throw new CurrencyApiQuotaLimitReachedException();
                }
                
                response.EnsureSuccessStatusCode();       
                
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var currencyData = JsonSerializer.Deserialize<CurrencyModel>(jsonContent);
            
            if (currencyData == null || string.IsNullOrEmpty(currencyData.BaseCurrency))
            {
                _logger.LogWarning("Invalid currency code: {Currency}", currency);
                throw new InvalidCurrencyException(currency);
            }
            
            return currencyData;
        }
        catch (InvalidCurrencyException)
        {
            throw;
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Invalid currency code: {Currency}", currency);
            throw new InvalidCurrencyException(currency, $"Currency code '{currency}' is not supported.", e);
        }
        catch (CurrencyApiQuotaLimitReachedException)
        {
            _logger.LogWarning("Currency API quota limit reached");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while fetching currency {Currency}", currency);
            throw new Exception($"An error occurred while fetching currency {currency}.", e);
        }
    }
}