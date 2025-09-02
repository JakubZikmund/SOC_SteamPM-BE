namespace SOC_SteamPM_BE.Models;

public enum DataStatus
{
    Loading,
    Ready,
    Updating,
    Error
}

public class GameDataState
{
    public DataStatus Status { get; set; } = DataStatus.Loading;
    public SteamGameResponse? Data { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public int UpdateAttempts { get; set; } = 0;
}
