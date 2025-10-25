using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Middleware;
using SOC_SteamPM_BE.Services;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.Engine;
using SOC_SteamPM_BE.Services.GameSearch;
using SOC_SteamPM_BE.Services.Initialization;
using SOC_SteamPM_BE.Services.Steam;

namespace SOC_SteamPM_BE;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Přidávání vlastních servisů
        // TRANSIENT: New instance every time 
        // builder.Services.AddTransient<IEmailService, EmailService>();

        // SCOPED: One per request 
        // builder.Services.AddScoped<IGameDataService, GameDataService>();

        // SINGLETON: One for entire app 
        // builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        
        // Core services
        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();

        // HTTP client for external API calls
        builder.Services.AddHttpClient();

        // This is used to bind the configuration to the SteamApiSettings and DataStorageSettings classes
        // from the appsettings.json file or other configuration sources.
        // Configuration binding
        builder.Services.Configure<SteamApiSettings>(
            builder.Configuration.GetSection("SteamApi"));
        builder.Services.Configure<DataStorageSettings>(
            builder.Configuration.GetSection("DataStorage")); ;

        // Register our custom services
        builder.Services.AddSingleton<IEngineDataManager, EngineDataManager>(); // State management
        builder.Services.AddScoped<IEngineDataService, EngineDataService>(); // Facade service (backwards compatibility)
        
        // API and external services (Scoped - per request)
        builder.Services.AddScoped<ISteamApiService, SteamApiService>(); // Steam API client
        
        // GameSearch services (Scoped - per request/operation)
        builder.Services.AddScoped<IGameSearchFileService, GameSearchFileService>(); // File I/O
        builder.Services.AddScoped<IGameSearchRefreshFromApiService, GameSearchRefreshFromApiService>(); // Refresh orchestration
        builder.Services.AddScoped<IGameSearchInitializationService, GameSearchInitializationService>(); // Startup initialization
        builder.Services.AddScoped<IGameSearchService, GameSearchService>(); // Facade service (backwards compatibility)
        
        
        // startup & background service
        builder.Services.AddHostedService<WebApiInitializationService>();
        builder.Services.AddHostedService<GameSearchSchedulerService>();
        
        // API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // My own middleware which checks if the service is ready
        // and if not, returns an error response
        app.UseEngineStatus();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }
}