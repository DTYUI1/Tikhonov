using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Repositories.Interfaces;

public interface IWeatherTypeRepository : IRepository<WeatherType>
{
    Task<WeatherType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}