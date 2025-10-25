using System.Collections.Concurrent;

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
    public Dictionary<string, SearchGameModel>? SteamGamesDict { get; set; }
    public DateTime SteamGamesDictLastUpdated { get; set; }
    public int SteamGamesDictUpdateAttempts { get; set; } = 0;
}

