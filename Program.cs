using SOC_SteamPM_BE.Middleware;
using SOC_SteamPM_BE.Services;
using SOC_SteamPM_BE.Models;

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
        builder.Services.AddSingleton<IGameCacheService, GameSearchCacheService>(); // Singleton for in-memory cache
        builder.Services.AddScoped<ISteamApiService, SteamApiService>(); // Scoped for API calls
        builder.Services.AddSingleton<IEngineDataManager, EngineDataManager>(); // Singleton for shared state
        builder.Services.AddScoped<IGameSearchService, GameSearchService>();
        
        
        // Register the background service
        builder.Services.AddHostedService<GameSearchRefreshService>();
        
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