namespace WeatherAPI.API.Models.DTO;

public record ErrorResponse(
    string Error,
    string Message,
    string TraceId,
    Dictionary<string, string[]>? ValidationErrors = null
);