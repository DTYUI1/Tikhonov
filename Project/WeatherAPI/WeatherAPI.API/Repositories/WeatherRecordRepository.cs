using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Repositories;

public class WeatherRecordRepository : Repository<WeatherRecord>, IWeatherRecordRepository
{
    public WeatherRecordRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<WeatherRecord> Items, int Total)> GetPagedAsync(
        WeatherRecordFilterQuery filter, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(wr => wr.City)
            .Include(wr => wr.WeatherType)
            .AsQueryable();

        // Применяем фильтры
        if (filter.CityId.HasValue)
        {
            query = query.Where(wr => wr.CityId == filter.CityId.Value);
        }

        if (filter.WeatherTypeId.HasValue)
        {
            query = query.Where(wr => wr.WeatherTypeId == filter.WeatherTypeId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(wr => wr.RecordedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(wr => wr.RecordedAt <= filter.ToDate.Value);
        }

        if (filter.MinTemperature.HasValue)
        {
            query = query.Where(wr => wr.Temperature >= filter.MinTemperature.Value);
        }

        if (filter.MaxTemperature.HasValue)
        {
            query = query.Where(wr => wr.Temperature <= filter.MaxTemperature.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(wr => wr.RecordedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<WeatherRecord?> GetLatestForCityAsync(
        Guid cityId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(wr => wr.City)
            .Include(wr => wr.WeatherType)
            .Where(wr => wr.CityId == cityId)
            .OrderByDescending(wr => wr.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WeatherRecord?> GetWithDetailsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(wr => wr.City)
            .Include(wr => wr.WeatherType)
            .FirstOrDefaultAsync(wr => wr.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WeatherRecord>> GetByCityIdAsync(
        Guid cityId, 
        DateTime? from = null, 
        DateTime? to = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(wr => wr.WeatherType)
            .Where(wr => wr.CityId == cityId);

        if (from.HasValue)
            query = query.Where(wr => wr.RecordedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(wr => wr.RecordedAt <= to.Value);

        return await query
            .OrderByDescending(wr => wr.RecordedAt)
            .ToListAsync(cancellationToken);
    }
}