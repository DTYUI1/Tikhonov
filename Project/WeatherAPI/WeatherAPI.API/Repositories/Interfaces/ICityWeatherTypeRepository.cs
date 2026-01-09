using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Repositories.Interfaces;

public interface ICityWeatherTypeRepository
{
    Task<CityWeatherType?> GetAsync(Guid cityId, Guid weatherTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CityWeatherType>> GetByCityIdAsync(Guid cityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CityWeatherType>> GetByWeatherTypeIdAsync(Guid weatherTypeId, CancellationToken cancellationToken = default);
    Task<CityWeatherType> AddAsync(CityWeatherType entity, CancellationToken cancellationToken = default);
    Task<CityWeatherType> UpdateAsync(CityWeatherType entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(CityWeatherType entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid cityId, Guid weatherTypeId, CancellationToken cancellationToken = default);
}