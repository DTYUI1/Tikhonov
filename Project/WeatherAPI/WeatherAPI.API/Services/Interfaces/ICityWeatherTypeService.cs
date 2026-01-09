using WeatherAPI.API.Models.DTO;

namespace WeatherAPI.API.Services.Interfaces;

public interface ICityWeatherTypeService
{
    Task<IEnumerable<CityWeatherTypeResponseDto>> GetByCityIdAsync(Guid cityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CityWeatherTypeResponseDto>> GetByWeatherTypeIdAsync(Guid weatherTypeId, CancellationToken cancellationToken = default);
    Task<CityWeatherTypeResponseDto> CreateAsync(CityWeatherTypeCreateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task<CityWeatherTypeResponseDto> UpdateAsync(Guid cityId, Guid weatherTypeId, CityWeatherTypeUpdateDto dto, string userRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid cityId, Guid weatherTypeId, string userRole, CancellationToken cancellationToken = default);
}