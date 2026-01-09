using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Только для POST запросов
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        // Проверяем наличие Idempotency-Key
        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey) || 
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var key = idempotencyKey.ToString();
        var requestPath = context.Request.Path.ToString();

        try
        {
            // Проверяем, есть ли уже такой ключ
            var existingKey = await dbContext.IdempotencyKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Key == key && k.RequestPath == requestPath);

            if (existingKey != null)
            {
                if (existingKey.ExpiresAt < DateTime.UtcNow)
                {
                    // Ключ истёк, удаляем
                    dbContext.IdempotencyKeys.Remove(existingKey);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    // Возвращаем сохранённый ответ
                    _logger.LogInformation("Returning cached response for idempotency key: {Key}", key);
                    
                    context.Response.StatusCode = existingKey.StatusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(existingKey.ResponseBody);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking idempotency key, continuing with request");
        }

        // Перехватываем response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Читаем response
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();

        // Сохраняем для idempotency (только успешные ответы)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            try
            {
                var newIdempotencyKey = new IdempotencyKey
                {
                    Key = key,
                    RequestPath = requestPath,
                    ResponseBody = responseText,
                    StatusCode = context.Response.StatusCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                dbContext.IdempotencyKeys.Add(newIdempotencyKey);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error saving idempotency key");
            }
        }

        // Копируем response обратно
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }
}
