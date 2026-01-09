using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Repositories;

public class CityRepository : Repository<City>, ICityRepository
{
    public CityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<City?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<City> Items, int Total)> GetPagedAsync(
        CityFilterQuery filter, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(search) || 
                c.Country.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            query = query.Where(c => c.Country.ToLower() == filter.Country.ToLower());
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<City?> GetByNameAndCountryAsync(
        string name, 
        string country, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => 
                c.Name.ToLower() == name.ToLower() && 
                c.Country.ToLower() == country.ToLower(), 
                cancellationToken);
    }

    public async Task<City?> GetWithWeatherTypesAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.CityWeatherTypes)
                .ThenInclude(cwt => cwt.WeatherType)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public override async Task<City> UpdateAsync(City entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
