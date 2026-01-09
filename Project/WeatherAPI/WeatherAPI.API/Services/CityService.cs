using Microsoft.EntityFrameworkCore;
using WeatherAPI.API.Data;
using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class CityService : ICityService
{
    private readonly ApplicationDbContext _context;
    private readonly ICityRepository _cityRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CityService> _logger;
    private const string CacheKeyPrefix = "city_";

    public CityService(
        ApplicationDbContext context,
        ICityRepository cityRepository,
        ICacheService cacheService,
        ILogger<CityService> logger)
    {
        _context = context;
        _cityRepository = cityRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<PagedResponse<CityResponseDto>> GetAllAsync(
        CityFilterQuery filter, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting cities with filter: Page={Page}, PageSize={PageSize}, Search={Search}", 
            filter.Page, filter.PageSize, filter.Search);

        var (cities, total) = await _cityRepository.GetPagedAsync(filter, cancellationToken);
        var items = cities.Select(MapToResponse).ToList();
        return new PagedResponse<CityResponseDto>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<CityDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        
        var cached = await _cacheService.GetAsync<CityDetailDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogInformation("City {CityId} retrieved from cache", id);
            return cached;
        }

        var city = await _cityRepository.GetWithWeatherTypesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"City with id {id} not found");

        var result = MapToDetailResponse(city);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);
        return result;
    }

    public async Task<CityResponseDto> CreateAsync(
        CityCreateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Create");
        _logger.LogInformation("Creating city: {CityName}, {Country}", dto.Name, dto.Country);

        var existing = await _cityRepository.GetByNameAndCountryAsync(dto.Name, dto.Country, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"City {dto.Name} in {dto.Country} already exists");
        }

        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            TimeZone = dto.TimeZone
        };

        await _cityRepository.AddAsync(city, cancellationToken);
        _logger.LogInformation("City created: {CityId}", city.Id);
        return MapToResponse(city);
    }

    public async Task<CityResponseDto> UpdateAsync(
        Guid id, 
        CityUpdateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Update");
        _logger.LogInformation("Updating city: {CityId}", id);

        // Получаем сущность напрямую из контекста для отслеживания
        var city = await _context.Cities.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"City with id {id} not found");

        // Проверяем уникальность только если изменилось название или страна
        if (!string.Equals(city.Name, dto.Name, StringComparison.OrdinalIgnoreCase) || 
            !string.Equals(city.Country, dto.Country, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _context.Cities.AsNoTracking()
                .FirstOrDefaultAsync(c => 
                    c.Name.ToLower() == dto.Name.ToLower() && 
                    c.Country.ToLower() == dto.Country.ToLower() &&
                    c.Id != id, cancellationToken);
            
            if (existing != null)
            {
                throw new InvalidOperationException($"City {dto.Name} in {dto.Country} already exists");
            }
        }

        // Обновляем поля
        city.Name = dto.Name;
        city.Country = dto.Country;
        city.Latitude = dto.Latitude;
        city.Longitude = dto.Longitude;
        city.TimeZone = dto.TimeZone;
        city.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}", cancellationToken);
        
        _logger.LogInformation("City updated: {CityId}", city.Id);
        return MapToResponse(city);
    }

    public async Task DeleteAsync(Guid id, string userRole, CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Delete");
        _logger.LogInformation("Deleting city: {CityId}", id);

        var city = await _context.Cities.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"City with id {id} not found");

        _context.Cities.Remove(city);
        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}", cancellationToken);
        
        _logger.LogInformation("City deleted: {CityId}", id);
    }

    private static void ValidateAccess(string userRole, string action)
    {
        var allowedRoles = action switch
        {
            "Create" => new[] { "Admin", "Manager" },
            "Update" => new[] { "Admin", "Manager" },
            "Delete" => new[] { "Admin" },
            _ => new[] { "Admin", "Manager", "User" }
        };

        if (!allowedRoles.Contains(userRole))
        {
            throw new UnauthorizedAccessException($"Role {userRole} is not allowed to {action} cities");
        }
    }

    private static CityResponseDto MapToResponse(City city)
    {
        return new CityResponseDto(
            city.Id,
            city.Name,
            city.Country,
            city.Latitude,
            city.Longitude,
            city.TimeZone,
            city.CreatedAt,
            city.UpdatedAt
        );
    }

    private static CityDetailDto MapToDetailResponse(City city)
    {
        var weatherTypes = city.CityWeatherTypes?
            .Select(cwt => new WeatherTypeFrequencyDto(
                cwt.WeatherTypeId,
                cwt.WeatherType?.Name ?? "Unknown",
                cwt.Frequency,
                cwt.Season
            ))
            .ToList() ?? new List<WeatherTypeFrequencyDto>();

        return new CityDetailDto(
            city.Id,
            city.Name,
            city.Country,
            city.Latitude,
            city.Longitude,
            city.TimeZone,
            city.CreatedAt,
            city.UpdatedAt,
            weatherTypes
        );
    }
}
