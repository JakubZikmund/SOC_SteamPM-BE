using Microsoft.AspNetCore.RateLimiting;
using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Middleware;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.Currencies;
using SOC_SteamPM_BE.Services.Engine;
using SOC_SteamPM_BE.Services.GameSearch;
using SOC_SteamPM_BE.Services.Initialization;
using SOC_SteamPM_BE.Services.PriceMap;
using SOC_SteamPM_BE.Services.Steam;
using SOC_SteamPM_BE.Services.Wishlist;

namespace SOC_SteamPM_BE;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load();
        
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
        
        // CORS policy - Allow all
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        
        // Configuration binding - appsettings.json
        builder.Services.Configure<SteamApiSettings>(
            builder.Configuration.GetSection("SteamApi"));
        builder.Services.Configure<CurrencySettings>(
            builder.Configuration.GetSection("CurrencyApi"));
        builder.Services.Configure<WishlistSettings>(
            builder.Configuration.GetSection("Wishlists"));

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
        builder.Services.AddScoped<IGameSearchRefreshFromApiService, GameSearchRefreshFromApiService>(); 
        builder.Services.AddScoped<IGameSearchInitializationService, GameSearchInitializationService>(); 
        builder.Services.AddScoped<IGameSearchService, GameSearchService>();
        
        // Wishlist service
        builder.Services.AddScoped<IWishlistService, WishlistService>();
        
        // startup & background service
        builder.Services.AddHostedService<WebApiInitializationService>();
        builder.Services.AddHostedService<GameSearchSchedulerService>();
        
        // API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Steam Price Map API",
                Version = "v1",
                Description = "API for searching Steam games and retrieving price information across different regions"
            });
            
            // Enable XML comments for Swagger documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("SteamPM", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 10;
                });
            });

        var app = builder.Build();

        // Middleware
        app.UseEngineStatus();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.MapControllers();

        app.Run();
    }
}