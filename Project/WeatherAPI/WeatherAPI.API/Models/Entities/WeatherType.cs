namespace WeatherAPI.API.Models.Entities;

public class WeatherType : BaseEntity
{
    public string Name { get; set; } = string.Empty;        // Sunny, Rainy, Cloudy, etc.
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;    // Код иконки погоды
    
    // Navigation properties
    public ICollection<WeatherRecord> WeatherRecords { get; set; } = new List<WeatherRecord>();
    public ICollection<CityWeatherType> CityWeatherTypes { get; set; } = new List<CityWeatherType>();
}