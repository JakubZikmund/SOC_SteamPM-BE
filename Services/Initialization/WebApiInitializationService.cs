using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.GameSearch;

namespace SOC_SteamPM_BE.Services.Initialization;

public class WebApiInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEngineDataManager _dataManager;
    private readonly ILogger<WebApiInitializationService> _logger;

    public WebApiInitializationService(
        IServiceProvider serviceProvider,
        IEngineDataManager dataManager,
        ILogger<WebApiInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _dataManager = dataManager;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Web API initialization starting...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dataManager = scope.ServiceProvider.GetRequiredService<IEngineDataManager>();
            var gameSearchInitService = scope.ServiceProvider.GetRequiredService<IGameSearchInitializationService>();

            // Inicializace data manageru
            await dataManager.InitializeAsync();

            // Inicializace game search dat
            await gameSearchInitService.InitializeAsync();
            
            // Nastavení data manageru na ready
            await dataManager.SetEngineStateAsync(EngineStatus.Ready);

            _logger.LogInformation("Web API initialization completed successfully. Status: Ready");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Web API initialization failed critically");
            
            Environment.Exit(1);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Web API shutting down...");
        return Task.CompletedTask;
    }
}
