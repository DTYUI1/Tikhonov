namespace WeatherAPI.API.Models.Entities;

/// <summary>
/// Связь many-to-many: какие типы погоды характерны для города
/// </summary>
public class CityWeatherType
{
    public Guid CityId { get; set; }
    public Guid WeatherTypeId { get; set; }
    public int Frequency { get; set; }              // Частота появления в %
    public string? Season { get; set; }             // Сезон: Winter, Spring, Summer, Autumn, All
    
    // Navigation properties
    public City City { get; set; } = null!;
    public WeatherType WeatherType { get; set; } = null!;
}