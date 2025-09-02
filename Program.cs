using SOC_SteamPM_BE.Services;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // TRANSIENT: New instance every time (like disposable cups)
        // builder.Services.AddTransient<IEmailService, EmailService>();

        // SCOPED: One per request (like one waiter per table)
        // builder.Services.AddScoped<IGameDataService, GameDataService>();

        // SINGLETON: One for entire app (like one manager for whole restaurant)
        // builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        
        // Core services
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();

        // HTTP client for external API calls
        builder.Services.AddHttpClient();

        // TODO: Vysvětlit jak funguje to Configure
        // Configuration binding
        builder.Services.Configure<SteamApiSettings>(
            builder.Configuration.GetSection("SteamApi"));
        builder.Services.Configure<DataStorageSettings>(
            builder.Configuration.GetSection("DataStorage"));

        // Register our custom services
        builder.Services.AddSingleton<IGameDataManager, GameDataManager>(); // Singleton for shared state
        builder.Services.AddScoped<IGameDataService, GameDataService>();
        
        // Register the background service
        builder.Services.AddHostedService<GameDataRefreshService>();

        // API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}