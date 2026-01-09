using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Repositories;

public class ApiKeyRepository : Repository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApiKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ak => ak.User)
            .FirstOrDefaultAsync(ak => ak.Key == key, cancellationToken);
    }

    public async Task<IEnumerable<ApiKey>> GetActiveKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ak => ak.IsActive && ak.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}