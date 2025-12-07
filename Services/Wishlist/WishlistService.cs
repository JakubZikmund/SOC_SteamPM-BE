using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.Steam;
using SOC_SteamPM_BE.Utils;

namespace SOC_SteamPM_BE.Services.Wishlist;

public interface IWishlistService
{
    Task<WishlistModel> GetPaginatedWishlistAsync(string steamId, int page = 1);
}

public class WishlistService : IWishlistService
{
    private readonly IEngineDataManager _dataManager;
    private readonly ISteamApiService _steamApi;
    private readonly IOptions<WishlistSettings> _wishlistSettings;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(IEngineDataManager dataManager, ISteamApiService steamApi, IOptions<WishlistSettings> wishlistSettings, ILogger<WishlistService> logger)
    {
        _dataManager = dataManager;
        _steamApi = steamApi;
        _wishlistSettings = wishlistSettings;
        _logger = logger;
    }

    public async Task<WishlistModel> GetPaginatedWishlistAsync(string steamId, int page = 1)
    {
        try
        {
            var wishlistItems = await _steamApi.FetchWishlistItems(steamId);

            var wishlistGames = new List<WishlistGame>();
            
            var pageSize = _wishlistSettings.Value.PageSize;
            
            if (wishlistItems.Count == 0)
            {
                return new WishlistModel(page, 0, wishlistGames);
            }

            for (int i = ((page - 1)*pageSize); i < (page*pageSize); i++)
            {
                if (i >= wishlistItems.Count) break;

                var item = wishlistItems[i];
                
                if (_dataManager.TryGetCachedWishlistGame(CacheKeys.Wishlist(item.AppId), out var gameInfo))
                {
                    wishlistGames.Add(new WishlistGame(gameInfo.Name, gameInfo.ImgUrl, item.AppId));
                }
                else
                {
                    var wishlistGameInfo = await _steamApi.FetchGameById(item.AppId, "cz");

                    var wishlistGame = new WishlistGame(wishlistGameInfo.Name, wishlistGameInfo.CapsuleImage, item.AppId);
                    
                    wishlistGames.Add(wishlistGame);
                    
                    // uložení do paměti
                    _dataManager.SetCachedWishlistGame(CacheKeys.Wishlist(item.AppId), wishlistGame);
                }
            }
            
            return new WishlistModel(page, wishlistItems.Count, wishlistGames);
        }
        catch (WishlistNotFoundException)
        {
            throw;
        }
        catch (SteamApiRateLimitExceededException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred while creating wishlist for steamId {steamId}", steamId);
            throw new Exception("An error occurred while creating wishlist.", e);
        }
    }
}