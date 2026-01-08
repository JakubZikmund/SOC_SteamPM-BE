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
    public SteamDateItem ReleaseDate {get; set;} = new();

    [JsonPropertyName("developers")]
    public List<string> Developers {get; set;} = new();

    [JsonPropertyName("publishers")]
    public List<string> Publishers {get; set;} = new();

    [JsonPropertyName("categories")]
    public List<SteamItem> Categories {get; set;} = new();

    [JsonPropertyName("genres")]
    public List<SteamItem> Genres {get; set;} = new();
    
    [JsonPropertyName("header_image")]
    public string HeaderImage {get; set;} = string.Empty;
    
    [JsonPropertyName("capsule_imagev5")]
    public string CapsuleImage {get; set;} = string.Empty;
    
    [JsonPropertyName("price_overview")]
    public SteamPrice? PriceOverview {get; set;} 
}

public class SteamItem
{
    [JsonPropertyName("description")]
    public string Description {get; set;} = string.Empty;
}

public class SteamDateItem
{
    [JsonPropertyName("date")]
    public string Date {get; set;} = string.Empty;
}

public class SteamPrice
{
    [JsonPropertyName("currency")]
    public string Currency {get; set;} = string.Empty;

    [JsonPropertyName("initial")]
    public decimal Initial {get; set;}
  
    [JsonPropertyName("discount_percent")]
    public int DiscountPercent {get; set;}

    [JsonPropertyName("final")]
    public decimal Final {get; set;}
}

public class GameInfo
{
    public GameInfo(string name, int appId, string shortDescription, string releaseDate, List<string> developers, List<string> publishers, string headerImage, string capsuleImage)
    {
        Name = name;
        AppId = appId;
        ShortDescription = shortDescription;
        ReleaseDate = releaseDate;
        Developers = developers;
        Publishers = publishers;
        Categories = new();
        Genres = new();
        HeaderImage = headerImage;
        CapsuleImage = capsuleImage;
    }

    public string Name {get; set;}
    public int AppId {get; set;}
    public string ShortDescription {get; set;}
    public string ReleaseDate {get; set;}
    public List<string> Developers {get; set;}
    public List<string> Publishers {get; set;}
    public List<string> Categories {get; set;}
    public List<string> Genres {get; set;}
    public string HeaderImage {get; set;} 
    public string CapsuleImage {get; set;} 
    
    public Dictionary<string, Price> PriceOverview {get; set;} = new();
}

public class Price
{
    public Price(int discountPercent, decimal final, decimal initial, decimal? convertedFinal = null, decimal? convertedInitial = null)
    {
        DiscountPercent = discountPercent;
        Initial = initial / 100m;
        Final = final / 100m;
        ConvertedInitial = convertedInitial;
        ConvertedFinal = convertedFinal;
    }

    public int DiscountPercent {get; set;}
    public decimal Initial { get; set; }
    public decimal Final {get; set;}
    public decimal? ConvertedInitial { get; set; }
    public decimal? ConvertedFinal { get; set; }
}
