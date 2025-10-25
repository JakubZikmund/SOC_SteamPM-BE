// Třída pro deserializaci ze Steam API

using System.Text.Json.Serialization;

public class SteamGameApiResponse
{
    [JsonPropertyName("name")]
    public string Name {get; set;} = string.Empty;
    
    [JsonPropertyName("steam_appid")]
    public int AppId {get; set;}
    
    [JsonPropertyName("short_description")]
    public string ShortDescription {get; set;} = string.Empty;
    
    [JsonPropertyName("release_date")]
    public SteamDateItem ReleaseDate {get; set;}
    
    [JsonPropertyName("developers")]
    public List<string> Developers {get; set;}
    
    [JsonPropertyName("publishers")]
    public List<string> Publishers {get; set;}
    
    [JsonPropertyName("categories")]
    public List<SteamItem> Categories {get; set;}
    
    [JsonPropertyName("genres")]
    public List<SteamItem> Genres {get; set;}
    
    [JsonPropertyName("header_image")]
    public string HeaderImage {get; set;} = string.Empty;
    
    [JsonPropertyName("price_overview")]
    public SteamPrice? PriceOverview {get; set;} // Jeden objekt ze Steam API
}

public class SteamItem
{
    [JsonPropertyName("description")]
    public string Description {get; set;} = string.Empty;
}

public class SteamDateItem
{
    [JsonPropertyName("date")]
    public string Description {get; set;} = string.Empty;
}

// Třída pro cenu ze Steam API (bez converted)
public class SteamPrice
{
    [JsonPropertyName("currency")]
    public string Currency {get; set;} = string.Empty;

    [JsonPropertyName("initial")]
    public int Initial {get; set;}

    [JsonPropertyName("discount_percent")]
    public int DiscountPercent {get; set;}

    [JsonPropertyName("final")]
    public int Final {get; set;}
}

// Tvoje interní třída s Dictionary
public class GameInfo
{
    public string Name {get; set;} = string.Empty;
    public int AppId {get; set;}
    public string ShortDescription {get; set;} = string.Empty;
    public string ReleaseDate {get; set;} = string.Empty;
    public List<string> Developers {get; set;}
    public List<string> Publishers {get; set;}
    public List<string> Categories {get; set;}
    public List<string> Genres {get; set;}
    public string HeaderImage {get; set;} = string.Empty;
    
    // Dictionary s tvými rozšířenými cenami
    public Dictionary<string, Price> PriceOverview {get; set;} = new();
}

// Tvoje rozšířená třída pro cenu (s converted)
public class Price
{
    public int DiscountPercent {get; set;}
    public int Final {get; set;}
    public int Initial {get; set;}
    
    // Tvůj vlastní atribut
    public int Converted {get; set;}
}
