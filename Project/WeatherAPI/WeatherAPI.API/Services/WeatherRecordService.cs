using WeatherAPI.API.Models.DTO;
using WeatherAPI.API.Models.Entities;
using WeatherAPI.API.Repositories.Interfaces;
using WeatherAPI.API.Services.Interfaces;

namespace WeatherAPI.API.Services;

public class WeatherRecordService : IWeatherRecordService
{
    private readonly IWeatherRecordRepository _weatherRecordRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IWeatherTypeRepository _weatherTypeRepository;
    private readonly IWeatherStatisticsRepository _statisticsRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<WeatherRecordService> _logger;

    public WeatherRecordService(
        IWeatherRecordRepository weatherRecordRepository,
        ICityRepository cityRepository,
        IWeatherTypeRepository weatherTypeRepository,
        IWeatherStatisticsRepository statisticsRepository,
        ICacheService cacheService,
        ILogger<WeatherRecordService> logger)
    {
        _weatherRecordRepository = weatherRecordRepository;
        _cityRepository = cityRepository;
        _weatherTypeRepository = weatherTypeRepository;
        _statisticsRepository = statisticsRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<PagedResponse<WeatherRecordResponseDto>> GetAllAsync(
        WeatherRecordFilterQuery filter, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting weather records with filters");

        var (records, total) = await _weatherRecordRepository.GetPagedAsync(filter, cancellationToken);

        var items = records.Select(MapToResponse).ToList();

        return new PagedResponse<WeatherRecordResponseDto>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<WeatherRecordResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _weatherRecordRepository.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather record with id {id} not found");

        return MapToResponse(record);
    }

    public async Task<CurrentWeatherDto?> GetCurrentWeatherAsync(Guid cityId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"current_weather_{cityId}";
        
        var cached = await _cacheService.GetAsync<CurrentWeatherDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var record = await _weatherRecordRepository.GetLatestForCityAsync(cityId, cancellationToken);
        
        if (record == null)
        {
            return null;
        }

        var result = new CurrentWeatherDto(
            record.City.Name,
            record.City.Country,
            record.WeatherType.Name,
            record.WeatherType.IconCode,
            record.Temperature,
            record.FeelsLike,
            record.Humidity,
            record.WindSpeed,
            record.RecordedAt
        );

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }

    public async Task<WeatherRecordResponseDto> CreateAsync(
        WeatherRecordCreateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Create");

        _logger.LogInformation("Creating weather record for city: {CityId}", dto.CityId);

        // Проверяем существование города и типа погоды
        if (!await _cityRepository.ExistsAsync(dto.CityId, cancellationToken))
        {
            throw new KeyNotFoundException($"City with id {dto.CityId} not found");
        }

        if (!await _weatherTypeRepository.ExistsAsync(dto.WeatherTypeId, cancellationToken))
        {
            throw new KeyNotFoundException($"Weather type with id {dto.WeatherTypeId} not found");
        }

        var record = new WeatherRecord
        {
            Id = Guid.NewGuid(),
            CityId = dto.CityId,
            WeatherTypeId = dto.WeatherTypeId,
            RecordedAt = dto.RecordedAt,
            Temperature = dto.Temperature,
            FeelsLike = dto.FeelsLike,
            Humidity = dto.Humidity,
            WindSpeed = dto.WindSpeed,
            WindDirection = dto.WindDirection,
            Pressure = dto.Pressure,
            Visibility = dto.Visibility
        };

        await _weatherRecordRepository.AddAsync(record, cancellationToken);

        // Инвалидируем кэш текущей погоды
        await _cacheService.RemoveAsync($"current_weather_{dto.CityId}", cancellationToken);

        _logger.LogInformation("Weather record created: {RecordId}", record.Id);

        // Получаем полную запись с навигационными свойствами
        var fullRecord = await _weatherRecordRepository.GetWithDetailsAsync(record.Id, cancellationToken);
        return MapToResponse(fullRecord!);
    }

    public async Task<WeatherRecordResponseDto> UpdateAsync(
        Guid id, 
        WeatherRecordUpdateDto dto, 
        string userRole, 
        CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Update");

        var record = await _weatherRecordRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather record with id {id} not found");

        var oldCityId = record.CityId;

        // Проверяем существование
        if (!await _cityRepository.ExistsAsync(dto.CityId, cancellationToken))
        {
            throw new KeyNotFoundException($"City with id {dto.CityId} not found");
        }

        if (!await _weatherTypeRepository.ExistsAsync(dto.WeatherTypeId, cancellationToken))
        {
            throw new KeyNotFoundException($"Weather type with id {dto.WeatherTypeId} not found");
        }

        record.CityId = dto.CityId;
        record.WeatherTypeId = dto.WeatherTypeId;
        record.RecordedAt = dto.RecordedAt;
        record.Temperature = dto.Temperature;
        record.FeelsLike = dto.FeelsLike;
        record.Humidity = dto.Humidity;
        record.WindSpeed = dto.WindSpeed;
        record.WindDirection = dto.WindDirection;
        record.Pressure = dto.Pressure;
        record.Visibility = dto.Visibility;

        await _weatherRecordRepository.UpdateAsync(record, cancellationToken);

        // Инвалидируем кэш
        await _cacheService.RemoveAsync($"current_weather_{oldCityId}", cancellationToken);
        await _cacheService.RemoveAsync($"current_weather_{dto.CityId}", cancellationToken);

        var fullRecord = await _weatherRecordRepository.GetWithDetailsAsync(record.Id, cancellationToken);
        return MapToResponse(fullRecord!);
    }

    public async Task DeleteAsync(Guid id, string userRole, CancellationToken cancellationToken = default)
    {
        ValidateAccess(userRole, "Delete");

        var record = await _weatherRecordRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Weather record with id {id} not found");

        var cityId = record.CityId;

        await _weatherRecordRepository.DeleteAsync(record, cancellationToken);

        await _cacheService.RemoveAsync($"current_weather_{cityId}", cancellationToken);

        _logger.LogInformation("Weather record deleted: {RecordId}", id);
    }

    public async Task<WeatherStatisticsDto> GetStatisticsAsync(
        Guid cityId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting weather statistics for city {CityId} from {From} to {To}", 
            cityId, from, to);

        return await _statisticsRepository.GetCityStatisticsAsync(cityId, from, to, cancellationToken);
    }

    public async Task<IEnumerable<DailyAverageDto>> GetDailyAveragesAsync(
        Guid cityId, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        return await _statisticsRepository.GetDailyAveragesAsync(cityId, from, to, cancellationToken);
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
            throw new UnauthorizedAccessException($"Role {userRole} is not allowed to {action} weather records");
        }
    }

    private static WeatherRecordResponseDto MapToResponse(WeatherRecord record)
    {
        return new WeatherRecordResponseDto(
            record.Id,
            record.CityId,
            record.City.Name,
            record.WeatherTypeId,
            record.WeatherType.Name,
            record.RecordedAt,
            record.Temperature,
            record.FeelsLike,
            record.Humidity,
            record.WindSpeed,
            record.WindDirection,
            record.Pressure,
            record.Visibility,
            record.CreatedAt
        );
    }
}