using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;

namespace WeatherAPI.API.Repositories;

public class WeatherTypeRepository : Repository<WeatherType>, IWeatherTypeRepository
{
    public WeatherTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WeatherType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(wt => wt.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}