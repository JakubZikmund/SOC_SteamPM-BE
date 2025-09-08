using Microsoft.Extensions.Options;

namespace SOC_SteamPM_BE.Services;

public class GameSearchRefreshService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameSearchRefreshService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public GameSearchRefreshService(
        IServiceProvider serviceProvider, 
        ILogger<GameSearchRefreshService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Game Data Refresh Service starting...");
        
        // Initialize data on startup
        using var scope = _serviceProvider.CreateScope();
        var gameSearchService = scope.ServiceProvider.GetRequiredService<IGameSearchService>();
        
        await ((GameSearchService)gameSearchService).InitializeAsync();
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game Data Refresh Service started and monitoring for midnight refresh");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1);
                var timeUntilMidnight = nextMidnight - now;

                // If it's close to midnight (within 1 minute), refresh
                if (timeUntilMidnight.TotalMinutes < 1)
                {
                    _logger.LogInformation("Midnight reached, starting scheduled refresh");
                    
                    using var scope = _serviceProvider.CreateScope();
                    var gameDataService = scope.ServiceProvider.GetRequiredService<IGameSearchService>();
                    
                    var success = await gameDataService.ForceRefreshAsync();
                    
                    if (success)
                    {
                        _logger.LogInformation("Scheduled midnight refresh completed successfully");
                    }
                    else
                    {
                        _logger.LogError("Scheduled midnight refresh failed after all attempts - using old data");
                    }
                    
                    // Wait 2 minutes to avoid multiple refreshes
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Game Data Refresh Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Game Data Refresh Service stopped");
    }
}

