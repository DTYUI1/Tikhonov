namespace WeatherAPI.API.Models.Entities;

public class City : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? TimeZone { get; set; }
    
    // Navigation properties
    public ICollection<WeatherRecord> WeatherRecords { get; set; } = new List<WeatherRecord>();
    public ICollection<CityWeatherType> CityWeatherTypes { get; set; } = new List<CityWeatherType>();
}