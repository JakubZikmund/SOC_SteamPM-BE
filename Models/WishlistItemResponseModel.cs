using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SOC_SteamPM_BE.Models;

public class WishlistItemResponseModel
{
    [JsonPropertyName("appid")]
    public int AppId {get; set;}
    
    [JsonPropertyName("priority")]
    public int Priority {get; set;}
}