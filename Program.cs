using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Middleware;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.Currencies;
using SOC_SteamPM_BE.Services.Engine;
using SOC_SteamPM_BE.Services.GameSearch;
using SOC_SteamPM_BE.Services.Initialization;
using SOC_SteamPM_BE.Services.PriceMap;
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
        
        // Configuration binding - appsettings.json
        builder.Services.Configure<SteamApiSettings>(
            builder.Configuration.GetSection("SteamApi"));
        builder.Services.Configure<DataStorageSettings>(
            builder.Configuration.GetSection("DataStorage"));
        builder.Services.Configure<CurrencySettings>(
            builder.Configuration.GetSection("CurrencyApi"));

        // Register our custom services
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IEngineDataManager, EngineDataManager>(); // State management
        builder.Services.AddScoped<IEngineDataService, EngineDataService>(); // Facade service (backwards compatibility)
        
        // API 
        builder.Services.AddScoped<ISteamApiService, SteamApiService>();
        builder.Services.AddScoped<ICurrencyApiService, CurrencyApiService>(); 
        
        // Price map services
        builder.Services.AddScoped<IPriceMapService, PriceMapService>();
        
        // Currency services
        builder.Services.AddScoped<ICurrencyService, CurrencyService>();
        
        // GameSearch services
        builder.Services.AddScoped<IGameSearchFileService, GameSearchFileService>(); 
        builder.Services.AddScoped<IGameSearchRefreshFromApiService, GameSearchRefreshFromApiService>(); 
        builder.Services.AddScoped<IGameSearchInitializationService, GameSearchInitializationService>(); 
        builder.Services.AddScoped<IGameSearchService, GameSearchService>();
        
        
        // startup & background service
        builder.Services.AddHostedService<WebApiInitializationService>();
        builder.Services.AddHostedService<GameSearchSchedulerService>();
        
        // API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Middleware
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