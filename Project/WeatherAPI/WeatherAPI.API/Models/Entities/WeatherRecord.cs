namespace WeatherAPI.API.Models.Entities;

public class WeatherRecord : BaseEntity
{
    public Guid CityId { get; set; }
    public Guid WeatherTypeId { get; set; }
    public DateTime RecordedAt { get; set; }
    public double Temperature { get; set; }          // В Цельсиях
    public double FeelsLike { get; set; }            // Ощущаемая температура
    public int Humidity { get; set; }                // Влажность в %
    public double WindSpeed { get; set; }            // Скорость ветра м/с
    public int? WindDirection { get; set; }          // Направление ветра в градусах
    public int? Pressure { get; set; }               // Давление в гПа
    public int? Visibility { get; set; }             // Видимость в метрах
    
    // Navigation properties
    public City City { get; set; } = null!;
    public WeatherType WeatherType { get; set; } = null!;
}