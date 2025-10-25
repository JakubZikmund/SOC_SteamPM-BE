using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.Engine;

public interface IEngineDataService
{
    EngineDataState GetCurrentState();   
}


public class EngineDataService : IEngineDataService
{
    private readonly IEngineDataManager _dataManager;
    
    public EngineDataService(IEngineDataManager dataManager)
    {
        _dataManager = dataManager;
    }
    
    public EngineDataState GetCurrentState()
    {
        return _dataManager.GetCurrentState();
    }
    
    
}