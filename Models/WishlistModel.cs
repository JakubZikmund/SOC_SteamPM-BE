namespace SOC_SteamPM_BE.Models;

public class WishlistModel
{
    public WishlistModel(int page, int wishlistSize, List<WishlistGame> games)
    {
        Page = page;
        WishlistSize = wishlistSize;
        Games = games;
    }

    public int Page { get; set; }
    public int WishlistSize { get; set; }
    public List<WishlistGame> Games { get; set; } 
}

public class WishlistGame
{
    public WishlistGame(string name, string imgUrl, int appId)
    {
        Name = name;
        ImgUrl = imgUrl;
        AppId = appId;
    }

    public string Name { get; set; }
    public string ImgUrl { get; set; }
    public int AppId { get; set; }
}