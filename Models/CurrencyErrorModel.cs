using System.Text.Json.Serialization;

namespace SOC_SteamPM_BE.Models;

public class CurrencyErrorModel
{
    [JsonPropertyName("result")]
    public string Result {get; set;} = String.Empty;
    
    [JsonPropertyName("error-type")]
    public string ErrorType {get; set;} = String.Empty;
}