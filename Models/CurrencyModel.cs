using System.Text.Json.Serialization;

namespace SOC_SteamPM_BE.Models;

public class CurrencyModel
{
    [JsonPropertyName("base_code")]
    public string BaseCurrency {get; set;} = String.Empty;
    
    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> ConversionalRates {get; set;}
}