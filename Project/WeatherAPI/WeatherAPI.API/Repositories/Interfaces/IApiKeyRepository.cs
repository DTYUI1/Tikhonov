using WeatherAPI.API.Models.Entities;

namespace WeatherAPI.API.Repositories.Interfaces;

public interface IApiKeyRepository : IRepository<ApiKey>
{
    Task<ApiKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKey>> GetActiveKeysAsync(CancellationToken cancellationToken = default);
}