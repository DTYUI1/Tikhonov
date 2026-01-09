using System.Net;
using System.Text.Json;
using FluentValidation;
using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception, "An error occurred. TraceId: {TraceId}", traceId);

        var (statusCode, errorResponse) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    "ValidationError",
                    "One or more validation errors occurred",
                    traceId,
                    validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                )
            ),
            
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                new ErrorResponse("NotFound", exception.Message, traceId)
            ),
            
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", exception.Message, traceId)
            ),
            
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("BadRequest", exception.Message, traceId)
            ),
            
            ArgumentException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("BadRequest", exception.Message, traceId)
            ),
            
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("InternalServerError", "An unexpected error occurred", traceId)
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}