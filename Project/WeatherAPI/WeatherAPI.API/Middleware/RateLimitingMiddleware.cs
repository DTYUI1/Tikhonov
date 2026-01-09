using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _clients = new();
    
    private const int RequestsPerMinute = 100;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var rateLimitInfo = _clients.GetOrAdd(clientId, _ => new RateLimitInfo
        {
            WindowStart = now,
            RequestCount = 0
        });

        lock (rateLimitInfo)
        {
            // Проверяем, не истекло ли временное окно
            if (now - rateLimitInfo.WindowStart >= TimeWindow)
            {
                rateLimitInfo.WindowStart = now;
                rateLimitInfo.RequestCount = 0;
            }

            rateLimitInfo.RequestCount++;

            // Добавляем заголовки rate limit
            context.Response.Headers["X-RateLimit-Limit"] = RequestsPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = 
                Math.Max(0, RequestsPerMinute - rateLimitInfo.RequestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = 
                ((long)(rateLimitInfo.WindowStart.AddMinutes(1) - DateTime.UnixEpoch).TotalSeconds).ToString();

            if (rateLimitInfo.RequestCount > RequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var error = new ErrorResponse(
                    "TooManyRequests",
                    "Rate limit exceeded. Please try again later.",
                    context.TraceIdentifier
                );
                
                var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                context.Response.WriteAsync(json).Wait();
                return;
            }
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Используем IP или API Key или User ID
        var apiKey = context.Request.Headers["X-API-KEY"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
            return $"apikey:{apiKey}";

        var userId = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    private class RateLimitInfo
    {
        public DateTime WindowStart { get; set; }
        public int RequestCount { get; set; }
    }
}