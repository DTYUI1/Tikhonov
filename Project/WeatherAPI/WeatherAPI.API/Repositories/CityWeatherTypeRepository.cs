using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Repositories;

public class CityWeatherTypeRepository : ICityWeatherTypeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<CityWeatherType> _dbSet;

    public CityWeatherTypeRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<CityWeatherType>();
    }

    public async Task<CityWeatherType?> GetAsync(
        Guid cityId, 
        Guid weatherTypeId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cwt => cwt.City)
            .Include(cwt => cwt.WeatherType)
            .FirstOrDefaultAsync(
                cwt => cwt.CityId == cityId && cwt.WeatherTypeId == weatherTypeId, 
                cancellationToken);
    }

    public async Task<IEnumerable<CityWeatherType>> GetByCityIdAsync(
        Guid cityId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cwt => cwt.WeatherType)
            .Where(cwt => cwt.CityId == cityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CityWeatherType>> GetByWeatherTypeIdAsync(
        Guid weatherTypeId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cwt => cwt.City)
            .Where(cwt => cwt.WeatherTypeId == weatherTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CityWeatherType> AddAsync(
        CityWeatherType entity, 
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<CityWeatherType> UpdateAsync(
        CityWeatherType entity, 
        CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(CityWeatherType entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid cityId, 
        Guid weatherTypeId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(cwt => cwt.CityId == cityId && cwt.WeatherTypeId == weatherTypeId, cancellationToken);
    }
}