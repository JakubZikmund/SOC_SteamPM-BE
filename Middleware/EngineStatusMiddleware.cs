using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services;
using System.Text.Json;
using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Services.Engine;

namespace SOC_SteamPM_BE.Middleware;

public class EngineStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EngineStatusMiddleware> _logger;

    public EngineStatusMiddleware(RequestDelegate next, ILogger<EngineStatusMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, IEngineDataManager engineDataManager)
    {
        var state = engineDataManager.GetCurrentState();
        
        if (state.Status != EngineStatus.Ready)
        {
            _logger.LogWarning("Game data not ready. Status: {Status}", state.Status);
            await WriteErrorResponse(context, state);
            return;
        }
        
        // Continue
        await _next(context);
    }

    private async Task WriteErrorResponse(HttpContext context, EngineDataState state)
    {
        var errorResponse = GetStatusError(state);
        
        context.Response.StatusCode = errorResponse.StatusCode;
        context.Response.ContentType = "application/json";
        
        var jsonResponse = JsonSerializer.Serialize(new 
        {
            error = errorResponse.Error,
            message = errorResponse.Message,
            status = errorResponse.Status
        });
        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponse GetStatusError(EngineDataState state)
    {
        return state.Status switch
        {
            EngineStatus.Updating => new ErrorResponse
            {
                StatusCode = 503,
                Error = "Service Updating",
                Message = "Game data is currently being updated. Please try again later.",
                Status = "updating"
            },
            EngineStatus.Loading => new ErrorResponse
            {
                StatusCode = 503,
                Error = "Service Loading",
                Message = "Game data is still loading. Please try again later.",
                Status = "loading"
            },
            EngineStatus.Error => new ErrorResponse
            {
                StatusCode = 500,
                Error = "Service Error",
                Message = state.ErrorMessage ?? "Game data is not available due to errors.",
                Status = "error"
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                Error = "Unknown Status",
                Message = "Game data service is in an unknown state.",
                Status = state.Status.ToString().ToLower()
            }
        };
    }

    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

// Extension method for easy registration
public static class EngineStatusMiddlewareExtensions
{
    public static IApplicationBuilder UseEngineStatus(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<EngineStatusMiddleware>();
    }
}