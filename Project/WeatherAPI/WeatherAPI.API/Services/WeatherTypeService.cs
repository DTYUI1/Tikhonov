using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class WeatherTypeService : IWeatherTypeService
{
    private readonly IWeatherTypeRepository _weatherTypeRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<WeatherTypeService> _logger;
    private const string AllWeatherTypesCacheKey = "weather_types_all";

    public WeatherTypeService(
        IWeatherTypeRepository weatherTypeRepository,
        ICacheService cacheService,
        ILogger<WeatherTypeService> logger)
    {
        _weatherTypeRepository = weatherTypeRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<WeatherTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<List<WeatherTypeResponseDto>>(
            AllWeatherTypesCacheKey, cancellationToken);
        
        if (cached != null)
        {
            _logger.LogInformation("Weather types retrieved from cache");
            return cached;
        }

        var weatherTypes = await _weatherTypeRepository.GetAllAsync(cancellationToken);
        var result = weatherTypes.Select(MapToResponse).ToList();

        await _cacheService.SetAsync(AllWeatherTypesCacheKey, result, TimeSpan.FromHours(1), cancellationToken);

        return result;
    }

    public async Task<WeatherTypeResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var weatherType = await _weatherTypeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather type with id {id} not found");

        return MapToResponse(weatherType);
    }

    public async Task<WeatherTypeResponseDto> CreateAsync(
        WeatherTypeCreateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Create");

        _logger.LogInformation("Creating weather type: {Name}", dto.Name);

        var existing = await _weatherTypeRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Weather type '{dto.Name}' already exists");
        }

        var weatherType = new WeatherType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IconCode = dto.IconCode
        };

        await _weatherTypeRepository.AddAsync(weatherType, cancellationToken);

        await _cacheService.RemoveAsync(AllWeatherTypesCacheKey, cancellationToken);

        _logger.LogInformation("Weather type created: {WeatherTypeId}", weatherType.Id);

        return MapToResponse(weatherType);
    }

    public async Task<WeatherTypeResponseDto> UpdateAsync(
        Guid id, 
        WeatherTypeUpdateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Update");

        var weatherType = await _weatherTypeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather type with id {id} not found");

        if (weatherType.Name != dto.Name)
        {
            var existing = await _weatherTypeRepository.GetByNameAsync(dto.Name, cancellationToken);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException($"Weather type '{dto.Name}' already exists");
            }
        }

        weatherType.Name = dto.Name;
        weatherType.Description = dto.Description;
        weatherType.IconCode = dto.IconCode;

        await _weatherTypeRepository.UpdateAsync(weatherType, cancellationToken);

        await _cacheService.RemoveAsync(AllWeatherTypesCacheKey, cancellationToken);

        return MapToResponse(weatherType);
    }

    public async Task DeleteAsync(Guid id, string userRole, CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Delete");

        var weatherType = await _weatherTypeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather type with id {id} not found");

        await _weatherTypeRepository.DeleteAsync(weatherType, cancellationToken);

        await _cacheService.RemoveAsync(AllWeatherTypesCacheKey, cancellationToken);

        _logger.LogInformation("Weather type deleted: {WeatherTypeId}", id);
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
            throw new UnauthorizedAccessException($"Role {userRole} is not allowed to {action} weather types");
        }
    }

    private static WeatherTypeResponseDto MapToResponse(WeatherType weatherType)
    {
        return new WeatherTypeResponseDto(
            weatherType.Id,
            weatherType.Name,
            weatherType.Description,
            weatherType.IconCode,
            weatherType.CreatedAt,
            weatherType.UpdatedAt
        );
    }
}