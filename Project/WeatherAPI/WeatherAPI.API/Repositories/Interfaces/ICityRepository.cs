using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Repositories.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<(IEnumerable<City> Items, int Total)> GetPagedAsync(
        CityFilterQuery filter, 
        CancellationToken cancellationToken = default);
    
    Task<City?> GetByNameAndCountryAsync(
        string name, 
        string country, 
        CancellationToken cancellationToken = default);
    
    Task<City?> GetWithWeatherTypesAsync(
        Guid id, 
        CancellationToken cancellationToken = default);
}