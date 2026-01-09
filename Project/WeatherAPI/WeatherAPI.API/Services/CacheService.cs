using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options, cancellationToken);

            _logger.LogDebug("Cached data for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Redis не поддерживает удаление по паттерну через IDistributedCache
        // Для полноценной реализации нужен IConnectionMultiplexer
        _logger.LogWarning("RemoveByPattern is not fully implemented for IDistributedCache");
        await Task.CompletedTask;
    }
}