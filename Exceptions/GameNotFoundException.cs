namespace SOC_SteamPM_BE.Exceptions;

public class GameNotFoundException : Exception
{
    public int AppId { get; }

    public GameNotFoundException(int appId) 
        : base($"Steam game with AppId {appId} was not found.")
    {
        AppId = appId;
    }

    public GameNotFoundException(int appId, string message) 
        : base(message)
    {
        AppId = appId;
    }

    public GameNotFoundException(int appId, string message, Exception innerException) 
        : base(message, innerException)
    {
        AppId = appId;
    }
}

