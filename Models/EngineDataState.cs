namespace SOC_SteamPM_BE.Models;

public enum EngineStatus
{
    Ready,
    Loading,
    Updating,
    Error
}

public class EngineDataState
{
    public EngineStatus Status { get; set; } = EngineStatus.Loading;
    public string? ErrorMessage { get; set; }
    public SteamGameResponse? GameSearchData { get; set; }
    public DateTime GameSearchLastUpdated { get; set; }
    public int GameSearchUpdateAttempts { get; set; } = 0;
}

