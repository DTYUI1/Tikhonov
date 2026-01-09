using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class CityWeatherTypeService : ICityWeatherTypeService
{
    private readonly ICityWeatherTypeRepository _cityWeatherTypeRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IWeatherTypeRepository _weatherTypeRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CityWeatherTypeService> _logger;

    public CityWeatherTypeService(
        ICityWeatherTypeRepository cityWeatherTypeRepository,
        ICityRepository cityRepository,
        IWeatherTypeRepository weatherTypeRepository,
        ICacheService cacheService,
        ILogger<CityWeatherTypeService> logger)
    {
        _cityWeatherTypeRepository = cityWeatherTypeRepository;
        _cityRepository = cityRepository;
        _weatherTypeRepository = weatherTypeRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<CityWeatherTypeResponseDto>> GetByCityIdAsync(
        Guid cityId, 
        CancellationToken cancellationToken = default)
    {
        var items = await _cityWeatherTypeRepository.GetByCityIdAsync(cityId, cancellationToken);
        
        var city = await _cityRepository.GetByIdAsync(cityId, cancellationToken);
        var cityName = city?.Name ?? "Unknown";

        return items.Select(cwt => new CityWeatherTypeResponseDto(
            cwt.CityId,
            cityName,
            cwt.WeatherTypeId,
            cwt.WeatherType.Name,
            cwt.Frequency,
            cwt.Season
        ));
    }

    public async Task<IEnumerable<CityWeatherTypeResponseDto>> GetByWeatherTypeIdAsync(
        Guid weatherTypeId, 
        CancellationToken cancellationToken = default)
    {
        var items = await _cityWeatherTypeRepository.GetByWeatherTypeIdAsync(weatherTypeId, cancellationToken);
        
        var weatherType = await _weatherTypeRepository.GetByIdAsync(weatherTypeId, cancellationToken);
        var weatherTypeName = weatherType?.Name ?? "Unknown";

        return items.Select(cwt => new CityWeatherTypeResponseDto(
            cwt.CityId,
            cwt.City.Name,
            cwt.WeatherTypeId,
            weatherTypeName,
            cwt.Frequency,
            cwt.Season
        ));
    }

    public async Task<CityWeatherTypeResponseDto> CreateAsync(
        CityWeatherTypeCreateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Create");

        _logger.LogInformation(
            "Creating city-weather type association: City={CityId}, WeatherType={WeatherTypeId}", 
            dto.CityId, dto.WeatherTypeId);

        // Проверяем существование
        var city = await _cityRepository.GetByIdAsync(dto.CityId, cancellationToken)
            ?? throw new KeyNotFoundException($"City with id {dto.CityId} not found");

        var weatherType = await _weatherTypeRepository.GetByIdAsync(dto.WeatherTypeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather type with id {dto.WeatherTypeId} not found");

        // Проверяем уникальность связи
        if (await _cityWeatherTypeRepository.ExistsAsync(dto.CityId, dto.WeatherTypeId, cancellationToken))
        {
            throw new InvalidOperationException("This city-weather type association already exists");
        }

        var entity = new CityWeatherType
        {
            CityId = dto.CityId,
            WeatherTypeId = dto.WeatherTypeId,
            Frequency = dto.Frequency,
            Season = dto.Season
        };

        await _cityWeatherTypeRepository.AddAsync(entity, cancellationToken);

        // Инвалидируем кэш города
        await _cacheService.RemoveAsync($"city_{dto.CityId}", cancellationToken);

        return new CityWeatherTypeResponseDto(
            entity.CityId,
            city.Name,
            entity.WeatherTypeId,
            weatherType.Name,
            entity.Frequency,
            entity.Season
        );
    }

    public async Task<CityWeatherTypeResponseDto> UpdateAsync(
        Guid cityId, 
        Guid weatherTypeId, 
        CityWeatherTypeUpdateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Update");

        var entity = await _cityWeatherTypeRepository.GetAsync(cityId, weatherTypeId, cancellationToken)
            ?? throw new KeyNotFoundException("City-weather type association not found");

        entity.Frequency = dto.Frequency;
        entity.Season = dto.Season;

        await _cityWeatherTypeRepository.UpdateAsync(entity, cancellationToken);

        await _cacheService.RemoveAsync($"city_{cityId}", cancellationToken);

        return new CityWeatherTypeResponseDto(
            entity.CityId,
            entity.City.Name,
            entity.WeatherTypeId,
            entity.WeatherType.Name,
            entity.Frequency,
            entity.Season
        );
    }

    public async Task DeleteAsync(
        Guid cityId, 
        Guid weatherTypeId, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Delete");

        var entity = await _cityWeatherTypeRepository.GetAsync(cityId, weatherTypeId, cancellationToken)
            ?? throw new KeyNotFoundException("City-weather type association not found");

        await _cityWeatherTypeRepository.DeleteAsync(entity, cancellationToken);

        await _cacheService.RemoveAsync($"city_{cityId}", cancellationToken);

        _logger.LogInformation(
            "Deleted city-weather type association: City={CityId}, WeatherType={WeatherTypeId}", 
            cityId, weatherTypeId);
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
            throw new UnauthorizedAccessException($"Role {userRole} is not allowed to {action}");
        }
    }
}