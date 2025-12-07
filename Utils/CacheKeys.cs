namespace SOC_SteamPM_BE.Utils;

public static class CacheKeys
{
    public static string Game(int appId) => $"game-{appId}";
    public static string Currency(string code) => $"curr-{code}";
    public static string Wishlist(int appId) => $"wishlist-{appId}";
}