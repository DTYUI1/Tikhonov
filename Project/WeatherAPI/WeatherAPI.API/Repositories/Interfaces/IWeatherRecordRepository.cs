using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Repositories.Interfaces;

public interface IWeatherRecordRepository : IRepository<WeatherRecord>
{
    Task<(IEnumerable<WeatherRecord> Items, int Total)> GetPagedAsync(
        WeatherRecordFilterQuery filter, 
        CancellationToken cancellationToken = default);
    
    Task<WeatherRecord?> GetLatestForCityAsync(
        Guid cityId, 
        CancellationToken cancellationToken = default);
    
    Task<WeatherRecord?> GetWithDetailsAsync(
        Guid id, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<WeatherRecord>> GetByCityIdAsync(
        Guid cityId, 
        DateTime? from = null, 
        DateTime? to = null, 
        CancellationToken cancellationToken = default);
}