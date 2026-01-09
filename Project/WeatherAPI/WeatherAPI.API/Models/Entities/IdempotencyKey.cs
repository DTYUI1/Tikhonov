namespace WeatherAPI.API.Models.Entities;

public class IdempotencyKey
{
    public string Key { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}