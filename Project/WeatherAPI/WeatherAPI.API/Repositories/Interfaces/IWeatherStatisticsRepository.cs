using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Repositories.Interfaces;

public interface IWeatherStatisticsRepository
{
    Task<WeatherStatisticsDto> GetCityStatisticsAsync(
        Guid cityId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DailyAverageDto>> GetDailyAveragesAsync(
        Guid cityId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default);
}

public record WeatherStatisticsDto(
    Guid CityId,
    string CityName,
    double AvgTemperature,
    double MinTemperature,
    double MaxTemperature,
    double AvgHumidity,
    double AvgWindSpeed,
    int RecordCount,
    string MostCommonWeatherType
);

public record DailyAverageDto(
    DateTime Date,
    double AvgTemperature,
    double MinTemperature,
    double MaxTemperature,
    double AvgHumidity
);