namespace SOC_SteamPM_BE.Services.GameSearch;

public class GameSearchSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameSearchSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public GameSearchSchedulerService(
        IServiceProvider serviceProvider, 
        ILogger<GameSearchSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game Search Scheduler Service started and monitoring for midnight refresh");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1);
                var timeUntilMidnight = nextMidnight - now;

                // If it is close to midnight (within 1 minute), refresh
                if (timeUntilMidnight.TotalMinutes < 1)
                {
                    _logger.LogInformation("Midnight reached, starting scheduled refresh");
                    
                    var refreshDataService = _serviceProvider.GetRequiredService<IGameSearchRefreshFromApiService>();
                    
                    var success = await refreshDataService.RefreshFromApiAsync();
                    
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
                _logger.LogError(ex, "Error in Game Search Scheduler Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Game Search Scheduler Service stopped");
    }
}

